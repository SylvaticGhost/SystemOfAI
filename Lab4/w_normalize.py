import numpy as np

w = np.array([
    6.083236, 5.737967, 16.35171, 0.51813793, 1.2349157, 1.8856012,
    8.547919, 8.85066, 8.397603, 8.921656, 8.118739, 8.5821495,
    8.706381, 8.237435, 9.013649, 8.400361, 9.262568, 9.005248,
    8.488785, 10.227346, 8.819691, 36.677753, 8.743167, 8.638772
])

avg = np.mean(w)
std = np.std(w)
median = np.median(w)
iqr = np.percentile(w, 75) - np.percentile(w, 25)
print(f'Average: {avg}, Standard Deviation: {std}, Median: {median}, IQR: {iqr}')

lower_bound = avg - iqr*1
upper_bound = avg + iqr*1

print(f'Lower bound: {lower_bound}, Upper bound: {upper_bound}')

w_cliped = np.clip(w, lower_bound, upper_bound)
print('Normalized weights:', w_cliped)