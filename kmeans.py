from mypoint import Centroid


def get_centroids(list_samples: list, k: int):
    centroids = []

    for i in range(k):
        sample = list_samples[i]

        centroid = Centroid(sample.x, sample.y)

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

            cent.list_samples.append(sample)

    return centroids

