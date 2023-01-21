import cv2
import numpy as np

from kmeans import get_centroids
from mypoint import Sample


def cluster_image(full_path: str):
    print("hello, world!")
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

    ##print(type(ret))
    ##print(type(dst))

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

    k = 55

    centroids = get_centroids(list_samples, k, num_rows=num_rows, num_cols=num_cols)

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

        # Line thickness of 9 px
        thickness = 3

        prev_point = None

        print(
            f'{centroid.index}, {len(centroid.list_samples)}, {len(centroid.convex_hull)}, {centroid.center.x}, {centroid.center.y}')

        center_point = (int(centroid.center.x), int(centroid.center.y))

        cv2.putText(img=image, text=f'{centroid.index}',
                    org=(center_point[0], center_point[1]), fontFace=cv2.FONT_HERSHEY_TRIPLEX, fontScale=1,
                    color=color, thickness=2)

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

        for curr_point in centroid.convex_hull:
            if prev_point:
                cv2.line(image, (prev_point.x, prev_point.y), (curr_point.x, curr_point.y), color=color, thickness=3)

            prev_point = curr_point

    ##print(f'list corners = {list_corners}')

    # Threshold for an optimal value.
    ##image[corners > 0.01 * corners.max()] = [0, 0, 255]

    ##print(f'corners = {corners}, {type(corners)}')

    # the window showing output image with corners
    cv2.imshow('Image with Corners', image)
    cv2.waitKey(0)
    cv2.destroyAllWindows()
