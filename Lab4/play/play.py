import gymnasium as gym
from ale_py import ALEInterface
import pygame
import numpy as np
import cv2
import numba
import time

# --- Constants for State Preprocessing ---
# (Copied directly from your notebook)
CONST_COLOR_PLAYER = (240, 170, 103)
CONST_COLOR_WALL = (84, 92, 214)
CONST_COLOR_ENEMY = (210, 210, 64)

CAT_EMPTY = 0
CAT_PLAYER = 1
CAT_WALL = 2
CAT_ENEMY = 3


# --- State Preprocessing Function (One-Hot) ---
# (Copied directly from your notebook)
@numba.njit
def prepare_state_categorical_inner_onehot(obs_resized, h, w):
    """Converts a 21x21x3 uint8 frame to a 21x21x4 one-hot state."""
    new_obs = np.full((h, w, 4), 0, dtype=np.uint8)
    for i in range(h):
        for j in range(w):
            pixel = obs_resized[i, j]
            if (pixel[0] == CONST_COLOR_PLAYER[0] and
                    pixel[1] == CONST_COLOR_PLAYER[1] and
                    pixel[2] == CONST_COLOR_PLAYER[2]):
                new_obs[i, j, CAT_PLAYER] = 1
            elif (pixel[0] == CONST_COLOR_WALL[0] and
                  pixel[1] == CONST_COLOR_WALL[1] and
                  pixel[2] == CONST_COLOR_WALL[2]):
                new_obs[i, j, CAT_WALL] = 1
            elif (pixel[0] == CONST_COLOR_ENEMY[0] and
                  pixel[1] == CONST_COLOR_ENEMY[1] and
                  pixel[2] == CONST_COLOR_ENEMY[2]):
                new_obs[i, j, CAT_ENEMY] = 1
            else:
                new_obs[i, j, CAT_EMPTY] = 1
    return new_obs


def prepare_state_categorical(frame, h=21, w=21):
    """Resize and one-hot encode a raw game frame."""
    obs_resized = cv2.resize(frame, (w, h), interpolation=cv2.INTER_NEAREST)
    return prepare_state_categorical_inner_onehot(obs_resized, h, w)


def get_human_action(keys):
    """Maps pygame key presses to the 18 Berzerk actions."""

    if keys[pygame.K_ESCAPE]:
        return "QUIT"

    # Check for arrow key presses OR WASD keys
    up = keys[pygame.K_UP] or keys[pygame.K_w]
    down = keys[pygame.K_DOWN] or keys[pygame.K_s]
    left = keys[pygame.K_LEFT] or keys[pygame.K_a]
    right = keys[pygame.K_RIGHT] or keys[pygame.K_d]

    # Check for fire key press
    fire = keys[pygame.K_SPACE]

    # --- Berzerk Action Mapping ---
    # Action indices from env.get_action_meanings()
    # 0: NOOP, 1: FIRE, 2: UP, 3: RIGHT, 4: LEFT, 5: DOWN
    # 6: UPRIGHT, 7: UPLEFT, 8: DOWNRIGHT, 9: DOWNLEFT
    # 10: UPFIRE, 11: RIGHTFIRE, 12: LEFTFIRE, 13: DOWNFIRE
    # 14: UPRIGHTFIRE, 15: UPLEFTFIRE, 16: DOWNRIGHTFIRE, 17: DOWNLEFTFIRE

    if fire:
        if up and right: return 14  # UPRIGHTFIRE
        if up and left:  return 15  # UPLEFTFIRE
        if down and right: return 16  # DOWNRIGHTFIRE
        if down and left:  return 17  # DOWNLEFTFIRE
        if up:    return 10  # UPFIRE
        if right: return 11  # RIGHTFIRE
        if left:  return 12  # LEFTFIRE
        if down:  return 13  # DOWNFIRE
        return 1  # FIRE (only)
    else:
        if up and right: return 6  # UPRIGHT
        if up and left:  return 7  # UPLEFT
        if down and right: return 8  # DOWNRIGHT
        if down and left:  return 9  # DOWNLEFT
        if up:    return 2  # UP
        if right: return 3  # RIGHT
        if left:  return 4  # LEFT
        if down:  return 5  # DOWN
        return 0  # NOOP (no keys pressed)


def record_episodes(num_episodes=5, output_file="expert_data.npz"):
    """
    Play the game for a number of episodes and save the trajectory data.
    """

    # Initialize Pygame and the environment
    pygame.init()

    ale = ALEInterface()
    gym.register_envs(ale)

    env = gym.make("ALE/Berzerk-v5", render_mode="human", frameskip=4)

    print("-" * 50)
    print("Berzerk Data Recorder Initialized.")
    print("Controls: Arrow keys OR WASD to move, SPACE to fire.")
    print(f"Recording {num_episodes} episodes...")
    print("The game window may take a moment to appear.")
    print("-" * 50)

    # List to hold all transitions (S, A, R, S', Done)
    trajectory_data = []

    for ep in range(num_episodes):
        print(f"Starting Episode {ep + 1}/{num_episodes}...")

        # Get the first raw state
        raw_state, _ = env.reset()

        # Preprocess it to get the agent's view
        state = prepare_state_categorical(raw_state)

        done = False
        total_reward = 0

        while not done:
            # 1. Update Pygame's event queue
            pygame.event.pump()

            # 2. Get keyboard state and map to action
            keys = pygame.key.get_pressed()
            action = get_human_action(keys)

            if action == "QUIT":
                print(f"Quit signal (ESC) received. Ending episode {ep + 1}.")
                done = True  # End this inner loop
                quit_recording = True  # Set flag to stop the outer loop
                continue

                # 3. Take the step in the environment
            next_raw_state, reward, terminated, truncated, _ = env.step(action)
            done = terminated or truncated

            # 4. Preprocess the *next* state
            next_state = prepare_state_categorical(next_raw_state)

            # 5. Store the transition
            trajectory_data.append((state, action, reward, next_state, done))

            # 6. Update for next loop
            state = next_state
            total_reward += reward

            # Optional: Add a small delay. The "human" render mode
            # usually caps at 60fps, and frameskip=4 means 15fps.
            # This sleep helps ensure we don't overwhelm the event pump.
            time.sleep(1 / 60)  # Sleep for 1/60th of a second

        print(f"Episode {ep + 1} finished. Total Score: {total_reward}")

    env.close()
    pygame.quit()

    print("\nRecording complete. Saving data...")

    # --- Save the Data ---
    # Convert list of tuples into separate NumPy arrays for efficiency
    try:
        states = np.array([t[0] for t in trajectory_data], dtype=np.uint8)
        actions = np.array([t[1] for t in trajectory_data], dtype=np.int32)
        rewards = np.array([t[2] for t in trajectory_data], dtype=np.float32)
        next_states = np.array([t[3] for t in trajectory_data], dtype=np.uint8)
        dones = np.array([t[4] for t in trajectory_data], dtype=bool)

        np.savez_compressed(output_file,
                            states=states,
                            actions=actions,
                            rewards=rewards,
                            next_states=next_states,
                            dones=dones)

        print(f"Successfully saved {len(trajectory_data)} transitions to {output_file}")

    except Exception as e:
        print(f"Error saving data: {e}")
        print("Saving as a raw .npy file as a fallback.")
        np.save(output_file.replace('.npz', '.npy'), trajectory_data)


if __name__ == "__main__":
    # Record 5 episodes and save to "expert_data.npz"
    record_episodes(num_episodes=5, output_file="expert_data.npz")