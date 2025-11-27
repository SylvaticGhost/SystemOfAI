import numpy as np

data = np.load('sarsa-weights-berzerk-10k-bad.npz')
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