## Description

Ce projet implique la création d'un **pipeline Azure DevOps** qui automatise le déploiement et l'exécution des ressources Azure. L'objectif principal est de configurer une **Azure Function App** qui traite des images lorsqu'elles sont téléchargées dans un **Storage Account**. Le nom du fichier est ensuite envoyé dans une **Queue Azure Service Bus** pour un traitement ultérieur (comme le redimensionnement ou l'ajout d'un watermark). Le fichier traité est ensuite déplacé et l'original est supprimé.

## Architecture

- **Compte de stockage Azure** : Stocke les images qui déclenchent l'Azure Function lors de leur téléchargement.
- **Azure Function 1** : Déclenchée lors du téléchargement d'une image, elle envoie le nom du fichier dans la **Queue Azure Service Bus**.
- **Queue Azure Service Bus** : Sert de tampon pour la seconde fonction.
- **Azure Function 2** : Traite l'image lorsqu'elle est déclenchée par le message dans la queue, et déplace le fichier après traitement.

## Fonctionnalités

- **Déploiement automatisé** des ressources via des **pipelines Azure DevOps**.
- **Traitement des fichiers** déclenché par le téléversement de fichiers dans Azure Blob Storage.
- Intégration de **Azure Service Bus** pour gérer les tâches de traitement asynchrone.

## Workflow

1. **Téléchargement de l'image** : Une image est téléchargée dans le **Storage Account**.
2. **Déclenchement de la fonction 1** : Cette fonction est déclenchée par l'événement de téléchargement du fichier et envoie le nom du fichier dans la **Queue Service Bus**.
3. **Déclenchement de la fonction 2** : La fonction 2 est déclenchée par le message dans la **Queue Service Bus**, traite l'image et la déplace dans un nouvel emplacement.

## Outils utilisés

- [Azure Functions](https://azure.microsoft.com/fr-fr/services/functions/)
- [Azure Storage Account](https://azure.microsoft.com/fr-fr/services/storage/)
- [Azure Service Bus](https://azure.microsoft.com/fr-fr/services/service-bus/)
- [Azure DevOps](https://azure.microsoft.com/fr-fr/services/devops/)
