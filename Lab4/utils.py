import numpy as np


def file_exist(file_name):
    try:
        _ = np.load(file_name)
        return True
    except FileNotFoundError:
        return False