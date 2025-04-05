import numpy as np

import matplotlib
matplotlib.use('TkAgg')

import matplotlib.pyplot as plt

X = np.array([
    [-4,  0],
    [ 2,  4],
    [ 1,  2],
    [ -2,  -1],
    [ 1,  -1]
], dtype=float)

T = np.array([0, 0, 1, 1, 0])


np.random.seed(42)
w = np.random.uniform(-0.5, 0.5, size=2)
b = np.random.uniform(-0.5, 0.5)
eta = 0.5

def step_activation(net):
    return 1 if net >= 0 else 0


for i in range(len(X)):
    x_i = X[i]
    net = w @ x_i + b
    y_p = step_activation(net)
    print("Вихід: ", y_p)


x_min = X[:, 0].min() - 1
x_max = X[:, 0].max() + 1
x_vals = np.linspace(x_min, x_max, 100)

# Якщо w[1] != 0:
y_vals = -(b + w[0]*x_vals) / w[1]

plt.figure(figsize=(8,6))

plt.scatter(X[:, 0], X[:, 1], c=T, cmap='bwr', edgecolors='k', s=80)

plt.plot(x_vals, y_vals, 'g-', label='Лінія розділу')

plt.xlabel('x1')
plt.ylabel('x2')
plt.title('Одношаровий персептрон: Лінія розділу та вхідні точки')
plt.legend()
plt.grid(True)
plt.show()