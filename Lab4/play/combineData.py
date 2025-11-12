import numpy as np
import glob


def combine_datasets(file_pattern="expert_data*.npz", output_file="combined_expert_data.npz"):
    """
    Finds all .npz files matching a pattern, combines their data,
    and saves them as a new compressed .npz file.
    """

    # Find all files that match the pattern (e.g., "expert_data_1.npz", "expert_data_2.npz")
    file_list = glob.glob(file_pattern)

    if not file_list:
        print(f"No files found matching pattern: {file_pattern}")
        return

    print(f"Found {len(file_list)} files to combine:")
    for f in file_list:
        print(f"  - {f}")

    # Lists to hold the data from all files
    all_states = []
    all_actions = []
    all_rewards = []
    all_next_states = []
    all_dones = []

    total_transitions = 0

    # Loop through each file and load its data
    for file_path in file_list:
        try:
            data = np.load(file_path)

            num_transitions = len(data['actions'])
            print(f"Loading {num_transitions} transitions from {file_path}...")

            all_states.append(data['states'])
            all_actions.append(data['actions'])
            all_rewards.append(data['rewards'])
            all_next_states.append(data['next_states'])
            all_dones.append(data['dones'])

            total_transitions += num_transitions

        except Exception as e:
            print(f"Error loading {file_path}: {e}")

    if not all_actions:
        print("No data was loaded. Exiting.")
        return

    # --- Combine all the data into single giant arrays ---
    print("\nCombining data...")

    # Use np.concatenate to stack the arrays
    combined_states = np.concatenate(all_states, axis=0)
    combined_actions = np.concatenate(all_actions, axis=0)
    combined_rewards = np.concatenate(all_rewards, axis=0)
    combined_next_states = np.concatenate(all_next_states, axis=0)
    combined_dones = np.concatenate(all_dones, axis=0)

    print("Combining complete.")

    # --- Save the new master file ---
    print(f"Saving {total_transitions} total transitions to {output_file}...")

    np.savez_compressed(output_file,
                        states=combined_states,
                        actions=combined_actions,
                        rewards=combined_rewards,
                        next_states=combined_next_states,
                        dones=combined_dones)

    print("\n--- Report ---")
    print(f"Total files combined: {len(file_list)}")
    print(f"Total transitions saved: {total_transitions}")
    print(f"States array shape: {combined_states.shape}")
    print(f"Actions array shape: {combined_actions.shape}")
    print(f"File saved to: {output_file}")


if __name__ == "__main__":
    # Assumes your files are named "expert_data_1.npz", "expert_data_2.npz", etc.
    combine_datasets(file_pattern="expert_data*.npz",
                     output_file="combined_expert_data.npz")