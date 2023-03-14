# Clustering Optimization using k-means

In this Project we use two approaches for the clustering problem.
The program takes images and automatycly cluster the samples in the image to k clusters
Our two approaches are:
- Clustering using sklearn k-means alforithem with selection of the k using the elnow methode
- Clustering using our hybrid k-means with standard deviation
## Getting Started

These instructions will get you a copy of the project up and running on your local machine.

### Prerequisites

- git
- python 3

### Installing

Clone the project

```
git clone https://github.com/shohamyamin/employee-app.git
cd clustering

```

Install packeges

```
pip install requirements.txt

```

## Run the program:


run the main.py file

```
python .\main.py
```
# Add more images to the dataset

The code runs on all the photos in the image directory.

so for adding images to the dataset that the program use just add youre images to the images directory and the code will use them.

## Results

The results from the progrem is in the results folder
The results include for evrey image the clustering visualization
For the k-means algorithem we show how the k is shifting and the optimal k that is selected using the Elbow Method

## Built With

- [python](https://www.python.org/)

## Additional libraries

- [open-cv](https://pypi.org/project/opencv-python/)
- [sklearn](https://pypi.org/project/scikit-learn/)

## Sources of information

- [towardsdatascience](https://towardsdatascience.com/elbow-method-is-not-sufficient-to-find-best-k-in-k-means-clustering-fc820da0631d) - Stop Using Elbow Method in K-means Clustering, Instead, Use this!
- [towardsdatascience](https://towardsdatascience.com/machine-learning-algorithms-part-9-k-means-example-in-python-f2ad05ed5203) - K-means Clustering Python Example
- [wikipedia](https://en.wikipedia.org/wiki/Stirling_numbers_of_the_second_kind)
- [UCA](https://faculty.uca.edu/ecelebi/documents/ESWA_2013.pdf) - A comparative study of efficient initialization methods for the k-means
clustering algorithm
- [GoodREADME](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2) - how to write a good README

## Authors

- **Hiam Lavi** - [HaimL76](https://github.com/HaimL76)
- **Shoham Yamin** - [shohamyamin](https://github.com/shohamyamin)
