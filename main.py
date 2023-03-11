from image import cluster_image_implemented, cluster_image_with_lib
import os
import glob
import csv

# Set the directory where your files are stored
image_directory_path = '.\clustering\images'

# Use glob to find all files in the directory
files = glob.glob(os.path.join(image_directory_path, '*.jpg'))
print(files)
results = []
# Define the path and filename for your CSV file
filename = r'''.\clustering\results.csv'''

for file in files:
    print(f'{file}')
    image_name = os.path.splitext(os.path.basename(file))[0]
    results.append(cluster_image_with_lib(f'{file}',k_iteration_index_quant=10, k_max=50, display_optimal_k=True))
    results.append(cluster_image_implemented(f'{file}', 3,random_mode=False))
    results.append(cluster_image_implemented(f'{file}', 3,random_mode=True))
    
# Open the CSV file for writing
with open(filename, 'a', newline='') as csvfile:

    # Define the fieldnames for your data
    fieldnames = ['Algorithem', 'Image', 'wcss','Optimal k','Initial Centroids']

    # Create a CSV writer object
    writer = csv.DictWriter(csvfile, fieldnames=fieldnames)

    # Write the header row to the CSV file
    writer.writeheader()

    # Write each row of data to the CSV file
    for row in results:
        writer.writerow(row)