from mypoint import Centroid


def get_centroids(list_samples: list, k: int):
    if not isinstance(list_samples, list) or len(list_samples) < 1:
        raise ValueError("list samples")

    centroids = []

    quant = len(list_samples) / k

    for i in range(k):
        sample = list_samples[i * quant]

        centroids[i] = Centroid(sample.x, sample.y)

    changed: int = 0

    for sample in list_samples:
        min0 = None
        cent = None

        for centroid in centroids:
            dx = sample.x - centroid.center.x
            dy = sample.y - centroid.center.y

            d = dx * dx + dy * dy

            if d < min0:
                min0 = d
                cent = centroid

        associated_centroid = sample.centroid

        if associated_centroid or associated_centroid != cent:
            changed += 1

        sample.centroid = cent
        cent.list_samples.append(sample)

    return centroids

