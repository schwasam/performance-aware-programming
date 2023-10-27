# ================================================================================
# Generate json test data for haversine calculation
# Based on real world cities
# ================================================================================

import json
import random

def loadPoints(filename):
    points = []
    with open(filename, 'r', encoding='utf-8') as file:
        jsonData = json.load(file)
        for location in jsonData:
            x = float(location["lat"])
            y = float(location["lng"])
            points.append([x, y])
    return points


def choosePairs(points, num_pairs):
    pairs = {"pairs": []}
    for i in range(num_pairs):
        if i % 1_000_000 == 0:
            percentage = i/num_pairs * 100.
            print("Status %d%%..." % percentage)

        start = random.choice(points)
        end = random.choice(points)
        pairs["pairs"].append({"x0": start[0], "y0": start[1], "x1": end[0], "y1": end[1]})
    return pairs


def dumpPairs(pairs):
    print("Dumping to file...")
    with open('./data/data_10000000_flex.json', 'w') as out:
        out.write(json.dumps(pairs))

cities = loadPoints("./data/cities.json")
num_pairs = 10_000_000
pairs = choosePairs(cities, num_pairs)
dumpPairs(pairs)
