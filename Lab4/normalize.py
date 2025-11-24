import numpy as np

data = np.load('sarsa-weights-berzerk.npz')
w = data['w']
print(f'Weights shape: {w.shape}')
print(w)

print(f'Example weights for state 0: {w[0]}')
non_zero_count = np.count_nonzero(w)
print(f'Non-zero count: {non_zero_count}, percentage: {non_zero_count / w.size * 100:.2f}%')

min_val = np.min(w)
max_val = np.max(w)
mean_val = np.mean(w)
std_val = np.std(w)
median_val = np.median(w)
q1_val = np.percentile(w, 25)
q3_val = np.percentile(w, 75)
iqr_val = q3_val - q1_val

lower_normal_bound = median_val - 1.5 * iqr_val
upper_normal_bound = median_val + 1.5 * iqr_val
outliers = w[(w < lower_normal_bound) | (w > upper_normal_bound)]
outlier_count = outliers.size

print(f'Min: {min_val}, Max: {max_val}')
print(f'Mean: {mean_val}, Std: {std_val}')
print(f'Median: {median_val}, Q1: {q1_val}, Q3: {q3_val}, IQR: {iqr_val}')
print(f'Lower normal bound: {lower_normal_bound}, Upper normal bound: {upper_normal_bound}')
print(f'Outlier count: {outlier_count}, Outlier percentage: {outlier_count / w.size * 100:.2f}%')

scale_range = upper_normal_bound - lower_normal_bound

if scale_range > 0:
    # Clip outliers to normal bounds
    w_normalized = np.clip(w, lower_normal_bound, upper_normal_bound)

    # Get the actual range of clipped weights
    w_min = np.min(w_normalized)
    w_max = np.max(w_normalized)
    w_range = w_max - w_min

    if w_range > 0:
        # Normalize to [0, 1] first
        w_temp = (w_normalized - w_min) / w_range

        # Scale back to [lower_normal_bound, upper_normal_bound]
        w_normalized = w_temp * scale_range + lower_normal_bound

        print(f'Normalized weights - Min: {np.min(w_normalized):.6f}, Max: {np.max(w_normalized):.6f}')
        print(f'Normalized weights - Mean: {np.mean(w_normalized):.6f}, Std: {np.std(w_normalized):.6f}')
        print(f'Target range: [{lower_normal_bound:.6f}, {upper_normal_bound:.6f}]')

        # Save normalized weights
        np.savez('sarsa-weights-berzerk-normalized.npz',
                 w=w_normalized,
                 b=data['b'],
                 n_actions=data['n_actions'],
                 state_dim=data['state_dim'])
        print('Normalized weights saved to sarsa-weights-berzerk-normalized.npz')
    else:
        print('Cannot normalize: all weights are identical after clipping')
else:
    print('Cannot normalize: scale range is zero or negative')