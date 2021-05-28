# CESI - Big Data - Téléchargement et reformattage des parties depuis les API de Riot Games

Dépôt _Git_ de l'application C# permettant de télécharger et de reformatter les parties jouées sur _League of Legends_ pour un profil donné. Applicaition réalisée dans le cadre du cours sur le Big Data du CESI.

## Introduction

Avant de démarrer l'application, il est nécessaire de récupérer au préalable le _PUUID_ du joueur ciblé, et de l'insérer dans le code de l'application. Même chose avec une clé d'API _Riot Games_ en cours de validité. Il est également possible de modifier les dossiers d'enregistrement des données téléchargées ou générées par le programme dans le code source. Toutes ces données sont modifiables des lignes 13 à 17 du fichier `Program.cs`.

Cette application permet de télécharger, au format CSV, toutes les parties disponibles depuis l'API de _Riot Games_ pour un profil donné. Les runes sont exclues des fichiers pour simplifier la conversion. Les données des parties sont téléchargées par l'application depuis les serveurs de _Riot Games_, au format JSON. Elles sont ensuite reformatées pour que chaque fichier concerne un seul joueur d'une seule partie. Une fois le traitement terminé, la conversion du JSON en CSV est effectuée.

## Dépendances

Il est nécessaire d'installer [_Visual Studio_](https://visualstudio.microsoft.com/fr) afin de pouvoir faire fonctionner ce projet. Il est également nécessaire d'installer des dépendances supplémentaires dans le projet grâce à _NuGet_.