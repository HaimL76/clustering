import cv2
import numpy as np

import pandas as pd
from matplotlib import pyplot as plt
#from sklearn.datasets.samples_generator import make_blobs
from sklearn.cluster import KMeans

from kmeans_implemented import KMeansImplemented
from mypoint import Sample, Centroid


def get_corners(full_path: str) -> list:
    print(full_path)
    import multiprocessing as mp
    print("Number of processors: ", mp.cpu_count())

    # Reading an image in default mode
    image = cv2.imread(full_path)

    # convert the input image into grayscale
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # modify the data type setting to 32-bit floating point
    gray = np.float32(gray)

    # apply the cv2.cornerHarris method to detect the corners
    corners = cv2.cornerHarris(gray, 2, 3, 0.05)

    # result is dilated for marking the corners
    ##corners = cv2.dilate(corners, None)

    ret, dst = cv2.threshold(corners, 0.01 * corners.max(), 255, 0)

    num_rows = corners.shape[0]
    num_cols = corners.shape[1]

    print(f'num rows = {num_rows}')
    print(f'num cols = {num_cols}')

    thresh = 0.01 * corners.max()

    list_samples: list = []

    for i in range(num_rows):
        for j in range(num_cols):
            if corners[i, j] > thresh:
                list_samples.append(Sample(j, i))

    return image, list_samples


def display_clusters(image, centroids: list, k: int):
    print(len(centroids))

    colors = 255 * 255 * 255

    quant_colors = colors / k

    size = 4

    half_size = size / 2

    for i in range(len(centroids)):
        centroid = centroids[i]

        color: int = i * quant_colors

        r: int = color % 255
        g: int = (color / 255) % 255
        b: int = (color / 255) / 255

        centroid.calculate_convex_hull()

        # Green color in BGR
        color = (r, g, b)
        color0 = (b, g, r)

        # Line thickness of 9 px
        thickness = 3

        prev_point = None

        if centroid.convex_hull:
            # print(f'{centroid.index}, {centroid.list_samples.get_count()}, {len(centroid.convex_hull)}, {centroid.center.x}, {centroid.center.y}')
            print(
                f'{centroid.index}, {len(centroid.list_samples)}, {len(centroid.convex_hull)}, {centroid.center.x}, {centroid.center.y}')

        center_point = (int(centroid.center.x), int(centroid.center.y))

        cv2.putText(img=image, text=f'{centroid.index}',
                    org=(center_point[0], center_point[1]), fontFace=cv2.FONT_HERSHEY_TRIPLEX, fontScale=1,
                    color=color, thickness=1)

        ##print(f'center point = {center_point}')

        ##print(f'{curr_point.x}, {curr_point.y}, {num_cols}, {num_rows}')

        #####if prev_point is not None:
        #######cv2.line(image, (prev_point.y, prev_point.x), (curr_point.y, curr_point.x), color=color, thickness=thickness)

        ##prev_point = curr_point

        font = cv2.FONT_HERSHEY_SIMPLEX

        ##cv2.putText(img=image, text=f'{centroid.index}, {center_point[0]}, {center_point[1]}', org=center_point, fontFace=cv2.FONT_HERSHEY_TRIPLEX, fontScale=1,
        ##          color=(0, 0, 255), thickness=1)

        for sample in centroid.list_samples:
            y = int(sample.y)
            x = int(sample.x)

            if False:  # y < num_rows and x < num_cols:
                cv2.putText(img=image, text=f'{centroid.index}',
                            org=(x, y), fontFace=cv2.FONT_HERSHEY_TRIPLEX, fontScale=1,
                            color=color, thickness=1)

        if isinstance(centroid.convex_hull, list) and len(centroid.convex_hull) > 2:
            for curr_point in centroid.convex_hull:
                if curr_point and prev_point:
                    cv2.line(image, (prev_point.x, prev_point.y), (curr_point.x, curr_point.y), color=color,
                             thickness=1)

                prev_point = curr_point

        if isinstance(centroid.diagonal_points, tuple) and len(centroid.diagonal_points) == 2:
            first_point: MyPoint = centroid.diagonal_points[0]
            second_point: MyPoint = centroid.diagonal_points[1]

            if curr_point and prev_point:
                cv2.line(image, (first_point.x, first_point.y), (second_point.x, second_point.y), color=color0,
                         thickness=1)

    ##print(f'list corners = {list_corners}')

    # Threshold for an optimal value.
    ##image[corners > 0.01 * corners.max()] = [0, 0, 255]

    ##print(f'corners = {corners}, {type(corners)}')

    # the window showing output image with corners
    cv2.imshow('Image with Corners', image)
    cv2.waitKey(0)
    cv2.destroyAllWindows()


def cluster_image_with_lib(full_path: str, k: int):
    tup: tuple = get_corners(full_path)

    image = tup[0]
    list_samples: list = tup[1]

    if isinstance(list_samples, list) and len(list_samples) > 0:
        list0: list = list(map(lambda sample: [sample.x, sample.y], list_samples))

        X = np.array(list0)

        centroids: list = []

        prev_inertia_ = None

        wcss = []

        for i in range(3, k):
            print(f'k: {i}')

            kmeans = KMeans(n_clusters=i, init='k-means++', max_iter=300, n_init=10, random_state=0)

            kmeans.fit(X)

            wcss.append(kmeans.inertia_)

            #if prev_inertia_ is not None:
             #   diff_inertia_ = prev_inertia_ - kmeans.inertia_
              #  ##print(f'inertia_: {kmeans.inertia_}, k: {i}, elbow = {prev_inertia_ - kmeans.inertia_}')

            #prev_inertia_ = kmeans.inertia_

            num_clusters: int = kmeans.n_clusters

            centroids = [None for j in range(num_clusters)]

            labels: list = list(kmeans.labels_)

            centers: list = list(kmeans.cluster_centers_)

            for index in range(len(labels)):
                sample: Sample = list_samples[index]

                label: int = labels[index]

                #print(f'label: {label}')

                centroid: Centroid = centroids[label]

                if centroid is None:
                    center = centers[label]
                    centroids[label] = Centroid(label, center[0], center[1])
                    centroid: Centroid = centroids[label]

                centroid.append_sample(sample)

        plt.plot(range(3, k), wcss)
        plt.title('Elbow Method')
        plt.xlabel('Number of clusters')
        plt.ylabel('WCSS')
        plt.show()

        display_clusters(image, centroids, k + 1)


def cluster_image_implemented(full_path: str, k: int, num_rows: int=0, num_cols: int=0):
    tup: tuple = get_corners(full_path)

    image = tup[0]
    list_samples: list = tup[1]

    if isinstance(list_samples, list) and len(list_samples) > 0:
        kmeans: KMeansImplemented = KMeansImplemented()

        centroids = kmeans.get_centroids(list_samples, k, num_rows=num_rows, num_cols=num_cols)

        if isinstance(centroids, list) and len(centroids) > 0:
            index: int = 0

            for centroid in centroids:
                centroid.index = index
                index += 1

            display_clusters(image, centroids, k)
