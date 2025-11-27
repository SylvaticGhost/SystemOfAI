import numpy as np

from Lab4.state import VECTOR_STATE_SIZE


class Sarsa:
    alpha = 1e-5
    gamma = 0.99
    epsilon = 1
    lmbda = 0.9
    weight_decay = 1e-5
    z_clip = 10.0

    def __init__(self, n_actions):
        self.state_dim = VECTOR_STATE_SIZE
        self.n_actions = n_actions
        self.w = np.zeros((self.n_actions, self.state_dim), dtype=np.float32)
        self.b = np.zeros(self.n_actions, dtype=np.float32)
        self.z_w = np.zeros_like(self.w, dtype=np.float32)
        self.z_b = np.zeros((self.n_actions,), dtype=np.float32)

    def phi_from_state_action(self, features, action):
        phi_w = np.zeros_like(self.w, dtype=np.float32)
        phi_w[action] = features.astype(np.float32)
        phi_b = np.zeros_like(self.b, dtype=np.float32)
        phi_b[action] = 1.0
        return phi_w, phi_b

    def _q_values_all_actions(self, state_features):
        return self.w.dot(state_features.astype(np.float32)) + self.b

    def epsilon_greedy(self, features):
        eps = float(getattr(self, "epsilon", 0.0))
        eps = max(0.0, min(1.0, eps))

        q_vals = self._q_values_all_actions(features)

        if np.random.rand() < eps:
            action = np.random.randint(self.n_actions)
        else:
            action = np.argmax(q_vals)

        return action, q_vals

    def save(self, file_name="sarsa_weights.npz"):
        np.savez(file_name, w=self.w.reshape(-1), b=self.b, n_actions=self.n_actions, state_dim=self.state_dim)

    def restrict_exploration(self):
        self.epsilon = 0.0

    def reset_traces(self):
        self.z_w.fill(0.0)
        self.z_b.fill(0.0)

    @staticmethod
    def load(file_name="sarsa_weights.npz"):
        data = np.load(file_name)
        n_actions = int(data['n_actions'])
        state_dim = int(data['state_dim'])

        if state_dim != VECTOR_STATE_SIZE:
            raise ValueError(
                f"Loaded weights have state_dim={state_dim}, "
                f"but current VECTOR_STATE_SIZE={VECTOR_STATE_SIZE}. "
                f"Delete `{file_name}` and retrain."
            )

        agent = Sarsa(n_actions)
        agent.state_dim = state_dim
        agent.w = data['w'].reshape((n_actions, state_dim)).astype(np.float32)
        agent.b = data['b'].astype(np.float32) if 'b' in data else np.zeros(n_actions, dtype=np.float32)
        agent.z_w = np.zeros_like(agent.w, dtype=np.float32)
        agent.z_b = np.zeros_like(agent.b, dtype=np.float32)
        return agent