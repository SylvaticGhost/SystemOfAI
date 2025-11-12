import numpy as np

data = np.load('sarsa-weights-berzerk-default-10k.npz')
w = data['w']
print(f'Weights shape: {w.shape}')

print(f'Example weights for state 0: {w[0]}')
print(f'Non-zero weights count: {np.count_nonzero(w)}')
