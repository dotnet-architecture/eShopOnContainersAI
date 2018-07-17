# eShopOnContainersAI 

## Definition and goals

This repo has a forked version of [https://github.com/dotnet-architecture/eShopOnContainers](eShopOnContainers) which has been evolved by adding AI and ML features.

*eShopOnContainers* is a cloud-native application based on microservices architecture and Docker containers.
*eShopOnContainersAI* is therefore a forked version of eShopOnContainers that is extended with AI features (Machine Learning and Deep Learning) plus a Bot client as a new client app which surfaces all the AI features along with the modified MVC web application.

Main AI/ML technologies used are:

- **ML.NET (Machine Learning .NET)**
- **Azure Cognitive Services (Computer Vision)**
- **TensorFlow / TensorFlowSharp**
- **CNTK**
- **Bot Framework**

Here's a vision of the architecture where the grayed area is what is coming derived from [https://github.com/dotnet-architecture/eShopOnContainers](eShopOnContainers) and the rest of the diagram is about the new added AI features.

![image](https://user-images.githubusercontent.com/1712635/42792118-bc21b18e-8928-11e8-9084-a5a6af87c8ba.png)

(*) Note that the architecture diagram is currently missing the ML.NET microservice, but the ML.NET scenario is explained in the Wiki.
This diagram will be updated soon.

The following diagram positions the multiple AI technologies per AI function and type:

![image](https://user-images.githubusercontent.com/1222398/36477436-746362e6-1701-11e8-9312-52faecbda715.png)

You will learn how to use Pre-Built models (such as in Cognitive Services), Pre-Trained and Custom models to add AI and ML features into any application:

*	Regression Models: These models are the most well-known and used around any kind of scenarios. Although they are very simple (compared with other models like deep neural networks) they are still the most used around the world. In eShopOnContainersAI we will use regression models to predict future product demand, training the algorithm with the order history data.
*	Recommendation systems: One of the most used cases, recommend products from the basket, will be used as example of these models.
*	Natural Language Processing: Bots are the corner stone of current AI applications. You will learn how to create new solutions based in BOT framework, integrate bots in your current applications, or use L.U.I.S. to get information about user intents, 
*	Computer Vision: These models gained much traction in current decade, and industry is investing large amount of resources in this field. Using different strategies, you will learn how to search for similar images, using Cognitive Services or deploying your own custom trained models.


See Wiki for how set it up and see the multiple scenarios:
https://github.com/dotnet-architecture/eShopOnContainersAI/wiki

## Sending feedback and pull requests
We'd appreciate your feedback, improvements and ideas.
You can create new issues at the issues section, do pull requests and/or send emails to **eshop_feedback@service.microsoft.com**


