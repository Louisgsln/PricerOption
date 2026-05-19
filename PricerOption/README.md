# OptionPricer 📈

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![xUnit Tests](https://img.shields.io/badge/Tests-xUnit-green.svg)](https://xunit.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**OptionPricer** est une application console .NET 8 modulaire et performante dédiée au pricing d'options financières européennes (Call & Put) en utilisant le modèle de **Black-Scholes-Merton (BSM)**. Il permet d'évaluer le prix théorique des contrats, de calculer analytiquement les indicateurs de sensibilité associés (les **Grecques**), et d'estimer la **volatilité implicite** à partir d'un prix de marché observé via une approche hybride robuste (Newton-Raphson avec repli par dichotomie).

Ce projet a été conçu selon des principes d'architecture propre, sans dépendance externe, ce qui en fait un excellent cas d'usage pédagogique et un projet idéal à présenter lors d'entretiens techniques pour des rôles de **Front-Office Developer** ou **Quant Developer junior**.

---

## 🚀 Fonctionnalités

1. **Modélisation de Contrats d'Options** (`OptionContract`) : Modèle de données robuste encapsulant les paramètres fondamentaux avec validation stricte à l'instanciation.
2. **Pricing Black-Scholes-Merton** (`BlackScholesPricer`) : Évaluation analytique rigoureuse gérant le rendement en dividende continu ($q$).
3. **Calculateur de Grecques** (`GreeksCalculator`) : Calcul exact de la sensibilité du prix de l'option par rapport aux paramètres sous-jacents (Delta, Gamma, Vega, Theta, Rho).
4. **Solveur de Volatilité Implicite** (`ImpliedVolatilitySolver`) :
   - Méthode principale rapide de **Newton-Raphson**.
   - Algorithme de repli (fallback) par **dichotomie (Bisection)** si les conditions de convergence ou de bornes ne sont pas respectées.
5. **Console Interactive** : Une interface utilisateur en ligne de commande intuitive, propre et colorée avec validation de saisie de données.
6. **Scénario d'Exemple** : Un mode démonstration préconfiguré exécutant l'évaluation d'une option classique pour valider instantanément les calculs du moteur quantitatif.

---

## 📐 Formules Mathématiques Utilisées

### 1. Pricing (Black-Scholes-Merton)

Pour un sous-jacent de prix actuel $S$, un strike $K$, un taux d'intérêt sans risque $r$, un rendement de dividende continu $q$, une maturité $T$, et une volatilité annualized $\sigma$ :

$$d_1 = \frac{\ln(S / K) + \left(r - q + \frac{\sigma^2}{2}\right) T}{\sigma \sqrt{T}}$$
$$d_2 = d_1 - \sigma \sqrt{T}$$

Le prix théorique est alors calculé par :
- **Call Price** : 
  $$C = S e^{-q T} N(d_1) - K e^{-r T} N(d_2)$$
- **Put Price** : 
  $$P = K e^{-r T} N(-d_2) - S e^{-q T} N(-d_1)$$

Où $N(x)$ représente la fonction de répartition de la loi normale standard cumulative (calculée de façon pure C# via l'approximation polynomiale hautement précise d'Abramowitz & Stegun avec une erreur maximale de $\pm 7.5 \times 10^{-8}$).

### 2. Les Grecques

Les indicateurs de sensibilité analytiques (Grecques) sont calculés ainsi :

| Grecque | Formule Call | Formule Put | Expression technique |
| :--- | :--- | :--- | :--- |
| **Delta ($\Delta$)** | $e^{-q T} N(d_1)$ | $-e^{-q T} N(-d_1)$ | Sensibilité brute au sous-jacent |
| **Gamma ($\Gamma$)** | $\frac{e^{-q T} \phi(d_1)}{S \sigma \sqrt{T}}$ | $\frac{e^{-q T} \phi(d_1)}{S \sigma \sqrt{T}}$ | Sensibilité de Delta au sous-jacent |
| **Vega ($\mathcal{V}$)** | $\frac{S e^{-q T} \phi(d_1) \sqrt{T}}{100}$ | $\frac{S e^{-q T} \phi(d_1) \sqrt{T}}{100}$ | Sensibilité à la volatilité (pour 1%) |
| **Theta ($\Theta$)** | $\frac{-\frac{S \sigma e^{-q T} \phi(d_1)}{2 \sqrt{T}} + q S e^{-q T} N(d_1) - r K e^{-r T} N(d_2)}{365}$ | $\frac{-\frac{S \sigma e^{-q T} \phi(d_1)}{2 \sqrt{T}} - q S e^{-q T} N(-d_1) + r K e^{-r T} N(-d_2)}{365}$ | Sensibilité au temps (par jour calendaire) |
| **Rho ($\rho$)** | $\frac{K T e^{-r T} N(d_2)}{100}$ | $\frac{-K T e^{-r T} N(-d_2)}{100}$ | Sensibilité aux taux d'intérêt (pour 1%) |

*Note : $\phi(x) = \frac{1}{\sqrt{2\pi}} e^{-\frac{x^2}{2}}$ est la fonction de densité de probabilité (PDF) normale standard.*

### 3. Volatilité Implicite

Le solveur recherche la racine $\sigma$ de l'équation :
$$f(\sigma) = \text{Price}(\sigma) - P_{\text{marché}} = 0$$

L'approximation de Newton-Raphson itère selon :
$$\sigma_{n+1} = \sigma_n - \frac{f(\sigma_n)}{f'(\sigma_n)} = \sigma_n - \frac{\text{Price}(\sigma_n) - P_{\text{marché}}}{\text{Vega}_{\text{annuelle}}(\sigma_n)}$$

Si $\text{Vega}$ devient trop faible ($< 10^{-7}$) ou si $\sigma_{n+1}$ sort des limites physiquement acceptables $[0.01\%, 500\%]$, l'algorithme bascule automatiquement vers la méthode robuste de la **dichotomie (Bisection)** pour garantir la convergence sous réserve que le prix de marché respecte les bornes d'arbitrage.

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
│       │   └── OptionType.cs
│       ├── Pricing/
│       │   ├── BlackScholesPricer.cs
│       │   └── IPricer.cs
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
        └── ImpliedVolatilitySolverTests.cs
```

---

## 🛠️ Instructions de Lancement

### Prérequis
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) installé sur votre machine.

### Cloner et compiler le projet

```bash
# Compiler la solution
dotnet build OptionPricer.sln
```

### Lancer l'application console

```bash
# Lancer le projet console interactif
dotnet run --project src/OptionPricer/OptionPricer.csproj
```

### Lancer la suite de tests

```bash
# Exécuter les tests unitaires avec xUnit
dotnet test OptionPricer.sln
```

---

## 📋 Exemple de Sortie Console (Sample Scenario)

Lors de la sélection de l'option `4. Run Sample Scenario` dans l'application, voici le rendu console attendu :

```text
--- Running Sample Scenario ---
Parameters:
  Spot = 100
  Strike = 100
  Maturity = 1.0 Year
  Risk-free rate = 5% (0.05)
  Volatility = 20% (0.20)
  Dividend Yield = 0%
  Option Type = Call

>>> Scenario Results <<<
Price         : 10.450580 (Expected: ~10.4506)
Delta         : 0.636831 (Expected: ~0.6368)
Gamma         : 0.018762 (Expected: ~0.0188)
Vega (1%)     : 0.375240 (Expected: ~0.3752)
Theta (1 day) : -0.017573 (Expected: ~-0.0176)
Rho (1%)      : 0.532321 (Expected: ~0.5323)
```

---

## ⚠️ Limites du Modèle Black-Scholes

Bien que le modèle de Black-Scholes soit le socle historique du pricing d'options, il comporte des hypothèses restrictives souvent contredites par la réalité des marchés (stylized facts) :
1. **Volatilité Constante** : Le modèle assume que la volatilité $\sigma$ du sous-jacent est constante dans le temps et identique pour tous les strikes, alors que les marchés affichent un **sourire/sourire déformé de volatilité (volatility smile/skew)**.
2. **Distribution Log-normale** : Le modèle présuppose des rendements gaussiens sans sauts. En réalité, les distributions empiriques de rendements présentent des queues épaisses (fat tails) et un pic plus prononcé (leucokurticité).
3. **Maturité Européenne Exclusive** : Il ne permet pas d'évaluer la possibilité d'exercice anticipé propre aux options de style américain.
4. **Taux sans risque constant** : Dans le modèle classique, $r$ est supposé déterministe et constant sur la durée de vie du contrat, ce qui fausse les valorisations à très longue maturité.

---

## 📈 Pistes d'Améliorations (Feuille de Route Quant)

Pour transformer ce projet simple en une bibliothèque quantitative industrielle :
- **Exercice Américain** : Implémenter un modèle d'évaluation par **Arbre Binomial** (ex: Cox-Ross-Rubinstein) ou par simulation **Monte-Carlo** avec l'algorithme des moindres carrés de **Longstaff-Schwartz (LSM)**.
- **Surface de Volatilité** : Modéliser une surface locale de volatilité (modèle de Dupire) ou interpoler la volatilité implicite via un modèle stochastique (SABR, Heston) à partir des prix du marché.
- **Calibration** : Développer un module de calibration pour ajuster les paramètres de modèles complexes (comme Heston) sur une grille de prix de marché en utilisant l'algorithme de Levenberg-Marquardt.
- **Interface Web API** : Envelopper ce moteur de calcul dans une **API ASP.NET Core** pour fournir des endpoints RESTful de pricing rapides.
- **Export de Données** : Ajouter une fonctionnalité d'export de rapports d'évaluation et de sensibilités au format CSV/Excel.
