from mypoint import Centroid, Sample


def get_centroids(list_samples: list, k: int, centroids: list = None, num_rows: int = 0, num_cols: int = 0):
    if not isinstance(list_samples, list) or len(list_samples) < 1:
        raise ValueError("list samples")

    if centroids is None:
        quant_rows = 0
        quant_cols = 0

        if num_rows > 0 and num_cols > 0:
            quant_rows = num_rows / k
            quant_cols = num_cols / k

        centroids = []

        quant = len(list_samples) / k

        for i in range(k):
            center = None

            if quant_rows > 0 and quant_cols > 0:
                center = Sample(i * quant_cols, i * quant_rows)
            else:
                center = list_samples[int(i * quant)]

            print(f'{i}, {center.x}, {center.y}')

            centroids.append(Centroid(i, center.x, center.y))

    changed: int = 0

    for sample in list_samples:
        min0 = None
        cent = None

        for centroid in centroids:
            dx = sample.x - centroid.center.x
            dy = sample.y - centroid.center.y

            d = dx * dx + dy * dy

            if min0 is None or d < min0:
                min0 = d
                cent = centroid

        associated_centroid = sample.centroid

        ##print(associated_centroid)

        if associated_centroid is None or associated_centroid != cent:
            changed += 1

            ##print(f'changed: {changed}')

        sample.centroid = cent
        cent.list_samples.append(sample)

        ##print(cent.index)

    print(f'changed = {changed}')

    if changed > 0:
        for centroid in centroids:
            centroid.calculate_center()
            centroid.list_samples = []

        return get_centroids(list_samples, k, centroids)

    return centroids