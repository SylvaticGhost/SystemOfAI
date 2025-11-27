import numpy as np
from ale_py import ALEInterface
import gymnasium as gym

from Lab4.state import State
from sarsa import Sarsa

agent = Sarsa.load("sarsa-weights-berzerk.npz")

ale = ALEInterface()
gym.register_envs(ale)

test_env = gym.make("ALE/Berzerk-v5", render_mode="human", frameskip=4)
agent.restrict_exploration()

n_episodes = 5
total_rewards = []

for ep in range(n_episodes):
    state, _ = test_env.reset()
    done = False
    ep_reward = 0

    actions_count = np.zeros(test_env.action_space.n, dtype=np.int32)
    while not done:
        feature_state = State(state)
        if feature_state.is_empty:
            action = 0
        else:
            action, _ = agent.epsilon_greedy(feature_state.as_vector())
        actions_count[action] += 1
        next_state, reward, terminated, truncated, _ = test_env.step(action)
        done = terminated or truncated

        state = next_state
        ep_reward += reward

    test_env.render()
    print(f"Episode {ep + 1}: Total Reward = {ep_reward}")
    print(f'Action count during round: {actions_count}')
    print('---------------------------')
    total_rewards.append(ep_reward)

test_env.close()

print(f"\nAverage Test Reward over {n_episodes} episodes: {np.mean(total_rewards):.2f}")