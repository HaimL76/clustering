from threading import Thread

from mypoint import Centroid, Sample


def centroid_calculate_center(centroid: Centroid):
    centroid.calculate_center()

    return centroid.index


class KMeans:
    def __init__(self):
        self.highest_index: int = 0
        self.list_centroids: list = []

    def clear_centroids(self):
        for centroid in self.list_centroids:
            centroid.list_samples = []

    def recalculate_centers(self):
        ts: list = []

        for centroid in self.list_centroids:
            ##centroid.calculate_convex_hull(calculate_center=True)
            centroid.calculate_center()

        if False:
            for centroid in self.list_centroids:
                t = Thread(target=centroid_calculate_center, args=[centroid])
                t.start()

            for t in ts:
                t.join()

    def set_highest_index(self):
        index: int = self.highest_index

        self.highest_index += 1

        return index

    def calculate_centroids_by_standard_deviations(self):
        number_centroids_changed: int = 0

        for centroid in self.list_centroids:
            new_centroids: list = centroid.get_new_centroids_by_standard_deviation()

            if isinstance(new_centroids, list) and len(new_centroids) > 0:
                for new_cent in new_centroids:
                    new_cent.index = self.set_highest_index()

                    self.list_centroids.append(new_cent)

                    number_centroids_changed += 1

                self.list_centroids.remove(centroid)
                centroid.list_samples = []

        return number_centroids_changed

    def associate_samples_to_centroids(self, list_samples: list):
        associations_changed: int = 0

        for sample in list_samples:
            min0 = None
            cent = None

            if not sample.disabled:
                for centroid in self.list_centroids:
                    dx = sample.x - centroid.center.x
                    dy = sample.y - centroid.center.y

                    d = dx * dx + dy * dy

                    if min0 is None or d < min0:
                        min0 = d
                        cent = centroid

                associated_centroid = sample.centroid

                if associated_centroid is None or associated_centroid != cent:
                    associations_changed += 1

                sample.centroid = cent
                sample.squared_distance_from_centroid = min0
                cent.append_sample(sample)

        return associations_changed

    def initialize_centroids(self, num_rows: int, num_cols: int, list_samples: list, k: int):
        if self.list_centroids is None or len(self.list_centroids) < 1:
            quant_rows = 0
            quant_cols = 0

            if num_rows > 0 and num_cols > 0:
                quant_rows = num_rows / k
                quant_cols = num_cols / k

            quant = len(list_samples) / k

            for i in range(k):
                center = None

                if quant_rows > 0 and quant_cols > 0:
                    center = Sample(i * quant_cols, i * quant_rows)
                else:
                    center = list_samples[int(i * quant)]

                print(f'{i}, {center.x}, {center.y}')

                self.list_centroids.append(Centroid(self.set_highest_index(), center.x, center.y))

    def calculate_centroids(self, list_samples: list, k: int, radius: float = 0, num_rows: int = 0, num_cols: int = 0):
        print("enter calculate centroids")

        if not isinstance(list_samples, list) or len(list_samples) < 1:
            return

        self.initialize_centroids(num_rows=num_rows, num_cols=num_cols, list_samples=list_samples, k=k)

        associations_changed: int = self.associate_samples_to_centroids(list_samples=list_samples)

        print(f'associations_changed = {associations_changed}')
        
        number_centroids_changed: int = self.calculate_centroids_by_standard_deviations()

        print(f'number_centroids_changed = {number_centroids_changed}')

        if associations_changed < 1 and number_centroids_changed < 1:
            return

        ##if number_centroids_changed > 0:
          ##  self.recalculate_centers()
           ## self.clear_centroids()
            ##self.calculate_centroids(list_samples, k, radius=radius, num_rows=num_rows, num_cols=num_cols)

        if associations_changed > 0:
            self.recalculate_centers()
            self.clear_centroids()
            self.calculate_centroids(list_samples, k, radius=radius, num_rows=num_rows, num_cols=num_cols)

    def get_centroids(self, list_samples: list, k: int, num_rows: int, num_cols: int):
        self.calculate_centroids(list_samples, k, num_rows=num_rows, num_cols=num_cols)

        return self.list_centroids
