from mypoint import Centroid


def get_centroids(list_samples: list, k: int, centroids: list = None):
    if not isinstance(list_samples, list) or len(list_samples) < 1:
        raise ValueError("list samples")

    if centroids is None:
        centroids = []

        quant = len(list_samples) / k

        for i in range(k):
            sample = list_samples[int(i * quant)]

            centroids.append(Centroid(i, sample.x, sample.y))

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
