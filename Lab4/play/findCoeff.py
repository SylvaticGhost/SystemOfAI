import numpy as np
import numba
from sklearn.svm import LinearSVC
import glob

# --- Constants for State Preprocessing ---
# (Must match your agent's preprocessing)
CAT_EMPTY = 0
CAT_PLAYER = 1
CAT_WALL = 2
CAT_ENEMY = 3
H, W = 21, 21

# Action 0 is NOOP, 2-9 are MOVE
MOVE_ACTIONS = [2, 3, 4, 5, 6, 7, 8, 9]


# --- Helper functions to analyze states ---
# These are needed to calculate our features.
@numba.njit
def _find_entities(state):
    """
    Finds player and enemies from a single (21, 21, 4) one-hot state.
    Returns player position as array and enemy coordinates as array.
    """
    # Find (y, x) coordinates where the player/enemy channel is 1
    player_coords = np.argwhere(state[:, :, CAT_PLAYER] == 1)
    enemy_coords = np.argwhere(state[:, :, CAT_ENEMY] == 1)

    # Return the first player position as array (or None), and enemy coords
    if len(player_coords) > 0:
        player_pos = player_coords[0]  # This is already a numpy array
    else:
        player_pos = None

    return player_pos, enemy_coords


@numba.njit
def _get_min_distance(player_pos, enemies):
    """Calculates min distance to an enemy."""
    if player_pos is None or len(enemies) == 0:
        return None

    min_dist = 999
    for i in range(len(enemies)):
        # Manhattan distance
        dist = abs(enemies[i, 0] - player_pos[0]) + abs(enemies[i, 1] - player_pos[1])
        if dist < min_dist:
            min_dist = dist
    return min_dist


# --- The Core Feature Engineering Function ---
@numba.njit
def extract_features(state, action, next_state):
    """
    Calculates the feature vector for a single (s, a, s') transition.
    This defines what the agent "cares" about.
    """
    mov_actions_local = [2, 3, 4, 5, 6, 7, 8, 9]
    # 1. Get info from the *current* state (s)
    prev_player_pos, prev_enemies = _find_entities(state)
    prev_num_enemies = len(prev_enemies)
    prev_min_distance = _get_min_distance(prev_player_pos, prev_enemies)

    # 2. Get info from the *next* state (s')
    player_pos, enemies = _find_entities(next_state)
    num_enemies = len(enemies)
    min_distance = _get_min_distance(player_pos, enemies)

    # 3. Calculate features based on (s, a, s')

    # f0: Inaction (NOOP or FIRE)
    f_inaction = 1.0 if action not in mov_actions_local else 0.0

    # f1: Wall Collision - check if player didn't move when they should have
    f_wall_collision = 0.0
    if (action in mov_actions_local and
        player_pos is not None and
        prev_player_pos is not None):
        # Check if positions are the same (didn't move)
        if (player_pos[0] == prev_player_pos[0] and
            player_pos[1] == prev_player_pos[1]):
            f_wall_collision = 1.0

    # f2: Kill Bonus
    f_kill = 1.0 if num_enemies < prev_num_enemies else 0.0

    # f3: Proximity Penalty
    f_proximity = 1.0 if (min_distance is not None and min_distance <= 2) else 0.0

    # f4: Hunting Bonus
    f_hunting = 1.0 if (prev_min_distance is not None and
                        min_distance is not None and
                        min_distance < prev_min_distance) else 0.0

    # f5: Death Penalty
    f_death = 1.0 if (player_pos is None and prev_player_pos is not None) else 0.0

    # f6: "Living Penalty" / Constant Bias
    # This feature is always on, allowing the model to learn a
    # constant negative (or positive) reward for just existing.
    f_bias = 1.0

    return np.array([
        f_inaction,
        f_wall_collision,
        f_kill,
        f_proximity,
        f_hunting,
        f_death,
        f_bias
    ], dtype=np.float32)


# --- Main IRL Function ---
def find_reward_weights(data_file="combined_expert_data.npz", num_actions=18):
    """
    Loads expert data and uses an SVM classifier to find the
    implied reward function weights.
    """
    print(f"Loading expert data from {data_file}...")
    try:
        data = np.load(data_file)
        states = data['states']
        actions = data['actions']
        next_states = data['next_states']
    except Exception as e:
        print(f"Error loading {data_file}: {e}")
        return

    num_transitions = len(actions)
    if num_transitions == 0:
        print("No transitions found.")
        return

    print(f"Loaded {num_transitions} expert transitions.")

    feature_vectors = []
    labels = []

    print("Generating feature vectors... This may take a moment.")

    # Process the data (this is the slowest part)
    for i in range(num_transitions):
        if i % 5000 == 0:
            print(f"  ...processing transition {i}/{num_transitions}")

        s = states[i]
        a_expert = actions[i]
        s_next = next_states[i]

        # 1. Add the "expert" feature vector
        phi_expert = extract_features(s, a_expert, s_next)
        feature_vectors.append(phi_expert)
        labels.append(1)  # Label as "good"

        # 2. Add "bad" feature vectors (all other actions)
        for a_bad in range(num_actions):
            if a_bad != a_expert:
                # We assume "bad" actions would also lead to s_next
                # This is a simplification, but effective
                phi_bad = extract_features(s, a_bad, s_next)
                feature_vectors.append(phi_bad)
                labels.append(0)  # Label as "bad"

    X = np.array(feature_vectors)
    y = np.array(labels)

    print(f"Generated {len(y)} total feature examples ({num_transitions} expert).")

    # --- Train the Classifier ---
    print("Training LinearSVC classifier to find reward weights...")

    # We use LinearSVC, a powerful linear classifier.
    # We set fit_intercept=False because our `f_bias` feature handles the intercept.
    # class_weight='balanced' helps because we have many more "bad" examples than "expert" ones.
    model = LinearSVC(fit_intercept=False, class_weight='balanced', max_iter=2000)
    model.fit(X, y)

    # The model's coefficients ARE the reward weights!
    weights = model.coef_[0]

    print("Training complete.")

    # --- Print the Report ---
    feature_names = [
        "Inaction (NOOP/FIRE)",
        "Wall Collision",
        "Kill Enemy",
        "Proximity Penalty (<= 2)",
        "Hunting (Closer to Enemy)",
        "Death",
        "Bias (Living Penalty)"
    ]

    print("\n--- 🏆 Implied Reward Weights ---")
    print("These are the 'penalties' and 'bonuses' learned from your gameplay:")

    for name, weight in zip(feature_names, weights):
        print(f"  - {name + ':':<25} {weight:+.4f}")

    return weights


if __name__ == "__main__":
    # Load the combined data and find the weights
    learned_weights = find_reward_weights(data_file="combined_expert_data.npz")