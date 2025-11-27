import numpy as np

FIRE_ACTIONS = [1, 10, 11, 12, 13, 14, 15, 16, 17]
MOVE_ACTIONS = [2, 3, 4, 5, 6, 7, 8, 9]

UP_DIRECTION = 0
RIGHT_DIRECTION = 1
LEFT_DIRECTION = 2
DOWN_DIRECTION = 3
UPRIGHT_DIRECTION = 4
UPLEFT_DIRECTION = 5
DOWNRIGHT_DIRECTION = 6
DOWNLEFT_DIRECTION = 7

ACTION_TO_DIRECTIONS = {
    2: [0],  # UP
    3: [1],  # RIGHT
    4: [2],  # LEFT
    5: [3],  # DOWN
    6: [0, 1],  # UPRIGHT
    7: [0, 2],  # UPLEFT
    8: [3, 1],  # DOWNRIGHT
    9: [3, 2],  # DOWNLEFT
}

EMPTY_COLOR = np.array([0, 0, 0], dtype=np.uint8)
WALL_COLOR = np.array([84, 92, 214], dtype=np.uint8)
ENEMY_COLOR = np.array([210, 210, 64], dtype=np.uint8)
PLAYER_COLOR = np.array([240, 170, 103], dtype=np.uint8)

EMPTY_INDEX = 0
WALL_INDEX = 1
ENEMY_INDEX = 2
PLAYER_INDEX = 3

PORTAL_INDEX = 4  # add manualy
PORTAL_COLOR = np.array([74, 255, 56], dtype=np.uint8)

AVG_PIXELS_IN_ENEMY = 74
MAX_ENEMIES = 8