# OptionPricer 📈

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![xUnit Tests](https://img.shields.io/badge/Tests-xUnit-green.svg)](https://xunit.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**OptionPricer** est une application console .NET 8 modulaire et performante dédiée au pricing d'options financières vanilla (**européennes** et **américaines**). Elle implémente trois grands moteurs de pricing reconnus de la finance quantitative, calcule analytiquement les indicateurs de sensibilité associés (les **Grecques**), et résout la **volatilité implicite** à partir d'un prix de marché observé via une approche hybride robuste.

Ce projet a été conçu selon des principes d'architecture propre, sans dépendance externe, ce qui en fait un excellent cas d'usage pédagogique et un projet idéal à présenter lors d'entretiens techniques pour des rôles de **Front-Office Developer** ou **Quant Developer junior**.

---

## 🚀 Fonctionnalités

1. **Modélisation de Contrats d'Options** (`OptionContract`) : Modèle de données robuste encapsulant les paramètres fondamentaux ($S, K, T, r, \sigma, q$) avec support des styles **Européen** et **Américain**.
2. **Multiples Moteurs de Pricing** :
   - **Moteur Black-Scholes-Merton** (`BlackScholesPricer`) : Évaluation analytique exacte pour les options européennes avec dividendes continus.
   - **Moteur Binomial (Cox-Ross-Rubinstein)** (`BinomialTreePricer`) : Modèle en treillis pour évaluer les options européennes et américaines (avec détection optimale de l'exercice anticipé à chaque nœud).
   - **Moteur Monte Carlo** (`MonteCarloPricer`) : Simulation de trajectoires d'actifs (mouvement brownien géométrique) avec technique de réduction de variance (variables antithétiques) et calcul d'erreur standard (Standard Error).
3. **Calculateur de Grecques** (`GreeksCalculator`) : Calcul exact de la sensibilité du prix de l'option par rapport aux paramètres sous-jacents (Delta, Gamma, Vega, Theta, Rho) pour le style européen.
4. **Solveur de Volatilité Implicite** (`ImpliedVolatilitySolver`) :
   - Méthode principale rapide de **Newton-Raphson**.
   - Algorithme de repli (fallback) par **dichotomie (Bisection)** si les conditions de convergence ne sont pas respectées.
5. **Console Interactive** : Interface utilisateur en ligne de commande intuitive, propre et colorée avec détection de redirection pour la robustesse (CI/CD).
6. **Comparateur de Modèles (Scénario d'Exemple)** : Mode démonstration préconfiguré qui price une option via les trois modèles simultanément et quantifie le **Premium d'exercice anticipé américain**.

---

## 📐 Formules Mathématiques Utilisées

### 1. Formule Fermée (Black-Scholes-Merton)

Pour un sous-jacent $S$, un strike $K$, un taux sans risque $r$, un dividende continu $q$, une maturité $T$, et une volatilité $\sigma$ :

$$d_1 = \frac{\ln(S / K) + \left(r - q + \frac{\sigma^2}{2}\right) T}{\sigma \sqrt{T}}$$
$$d_2 = d_1 - \sigma \sqrt{T}$$

Le prix théorique européen est calculé par :
- **Call Price** : $C = S e^{-q T} N(d_1) - K e^{-r T} N(d_2)$
- **Put Price** : $P = K e^{-r T} N(-d_2) - S e^{-q T} N(-d_1)$

*Note : $N(x)$ représente la fonction de répartition de la loi normale standard cumulative, calculée via l'approximation polynomiale hautement précise d'Abramowitz & Stegun.*

### 2. Arbre Binomial (Cox-Ross-Rubinstein)

L'arbre modélise l'évolution de l'actif par étapes de temps $\Delta t = T/N$ avec des facteurs de hausse ($u$) et de baisse ($d$) :

$$u = e^{\sigma \sqrt{\Delta t}}, \quad d = e^{-\sigma \sqrt{\Delta t}} = \frac{1}{u}$$
$$p = \frac{e^{(r - q)\Delta t} - d}{u - d}$$

À chaque étape $i$ (de l'échéance vers le début) et chaque nœud $j$ (nombre de hausses) :
- **Pour le style Européen** :
  $$V_{i, j} = e^{-r \Delta t} \left( p V_{i+1, j+1} + (1-p) V_{i+1, j} \right)$$
- **Pour le style Américain** (vérification de l'exercice anticipé) :
  $$V_{i, j} = \max\left(\text{Payoff}(S_{i, j}), e^{-r \Delta t} \left( p V_{i+1, j+1} + (1-p) V_{i+1, j} \right)\right)$$

### 3. Simulation de Monte Carlo

Simule $M$ trajectoires de l'actif sous la probabilité risque-neutre à l'aide de variables normales $Z_k \sim N(0, 1)$ générées par la transformation de Box-Muller :

$$S_T^{(k)} = S_0 \exp\left(\left(r - q - \frac{\sigma^2}{2}\right)T + \sigma \sqrt{T} Z_k\right)$$

Pour réduire la variance, la méthode des **variables antithétiques** est employée en doublant les trajectoires simulées via $-Z_k$. Le prix estimé est la moyenne actualisée des payoffs, et l'erreur type (Standard Error) de l'estimation est :

$$\text{SE} = \frac{\text{StDev}(\text{Payoffs})}{\sqrt{2M}}$$

---

## 💻 Structure du Projet

```text
OptionPricer/
│
├── OptionPricer.sln          # Fichier de solution Visual Studio
├── README.md                 # Documentation générale du projet
│
├── src/
│   └── OptionPricer/         # Projet Console (Moteur et interface)
│       ├── OptionPricer.csproj
│       ├── Program.cs         # Point d'entrée de l'application console
│       ├── Models/
│       │   ├── OptionContract.cs
│       │   ├── OptionStyle.cs  # [NEW] Enum European / American
│       │   └── OptionType.cs
│       ├── Pricing/
│       │   ├── IPricer.cs
│       │   ├── BlackScholesPricer.cs
│       │   ├── BinomialTreePricer.cs # [NEW] Modèle de treillis CRR
│       │   └── MonteCarloPricer.cs   # [NEW] Simulateur stochastique
│       ├── Greeks/
│       │   └── GreeksCalculator.cs
│       ├── Math/
│       │   └── NormalDistribution.cs
│       └── Solvers/
│           └── ImpliedVolatilitySolver.cs
│
└── tests/
    └── OptionPricer.Tests/   # Projet de Tests unitaires xUnit
        ├── OptionPricer.Tests.csproj
        ├── BlackScholesPricerTests.cs
        ├── GreeksCalculatorTests.cs
        ├── ImpliedVolatilitySolverTests.cs
        ├── BinomialTreePricerTests.cs # [NEW] Tests de convergence et d'exercice anticipé
        └── MonteCarloPricerTests.cs   # [NEW] Tests de convergence statistique (Standard Error)
```

---

## 🛠️ Instructions de Lancement

### Prérequis
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) installé.

### Compilation et Tests

```bash
# Restaurer et compiler la solution
dotnet build -c Release OptionPricer.sln

# Exécuter l'ensemble des tests unitaires
dotnet test OptionPricer.sln
```

### Lancement de l'application

```bash
dotnet run --project src/OptionPricer/OptionPricer.csproj
```

---

## 📋 Rendu de la Sortie Console (Mode Comparatif)

Lors de l'exécution du scénario de démonstration (choix `4` dans le menu), l'application réalise un benchmark croisé des modèles et met en évidence la prime américaine :

```text
--- Running Sample Scenario (Comparison & American Premium) ---
Base Parameters (European Call):
  Spot = 100, Strike = 100, Maturity = 1.0 Year, Rate = 5%, Vol = 20%, Div = 0%

>>> European Call Pricing Comparison <<<
  Pricing Model                  | Price      | Difference vs BSM 
--------------------------------------------------------------------
  Black-Scholes (Analytical)     | 10,450576  | Benchmark         
  Binomial Tree (CRR, 200 steps) | 10,440591  |          -0,009984
  Monte Carlo (100k paths)       | 10,464289  |           0,013714 (SE: ±0,046585)

American Early Exercise Premium Demonstration:
  Put Parameters: Spot = 100, Strike = 100, Maturity = 1.0 Year, Rate = 5%, Vol = 20%, Div = 0%

>>> American Put Premium Results <<<
  European Put Price (Black-Scholes) : 5,573518
  American Put Price (Binomial CRR)  : 6,086383
  American Early Exercise Premium    : 0,512865 (9,20 %)
```

---

## 📈 Pistes d'Améliorations (Feuille de Route Quant)

Pour étendre davantage ce moteur quantitatif :
- **Simulation d'Options Américaines par Monte-Carlo** : Implémenter l'algorithme des moindres carrés de **Longstaff-Schwartz (LSM)**.
- **Modélisation de Volatilité Stochastique** : Intégrer un modèle de type **Heston** pour capter le sourire de volatilité.
- **Options Exotiques** : Ajouter des pricers pour les options barrières, asiatiques ou lookback en étendant le moteur de Monte-Carlo.
- **Développement d'une API Web** : Exposer le moteur sous forme de micro-service via ASP.NET Core Minimal APIs.
