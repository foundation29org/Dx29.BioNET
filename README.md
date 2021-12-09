<div style="margin-bottom: 1%; padding-bottom: 2%;">
	<img align="right" width="100px" src="https://dx29.ai/assets/img/logo-Dx29.png">
</div>

Dx29 BioNET
==============================================================================================================================================

[![Build Status](https://f29.visualstudio.com/Dx29%20v2/_apis/build/status/DEV-MICROSERVICES/Dx29.BioNET?branchName=develop)](https://f29.visualstudio.com/Dx29%20v2/_build/latest?definitionId=110&branchName=develop)

### **Overview**

This project contains the Dx29 algorithm for the calculation and suggestion of diseases for a patient from his list of symptoms and/or genes.

It is used in the [Dx29 application](https://dx29.ai/) and therefore how to integrate it is described in the [Dx29 architecture guide](https://dx29-v2.readthedocs.io/en/latest/index.html).

It is programmed in C#, and the structure of the project is as follows:

>- src folder: This is made up of multimple folders which contains the source code of the project.
>>- Dx29.BioNET.Web.API. In this project is the implementation of the controllers that expose the API methods.
>>- Dx29.BioNET. It is this project that contains the logic to perform the relevant operations.
>>- Dx29 and Dx29.Azure. Used as libraries to add the common or more general functionalities used in Dx29 projects programmed in C#.
>- .gitignore file
>- README.md file
>- manifests folder: with the YAML configuration files for deploy in Azure Container Registry and Azure Kubernetes Service.
>- pipeline sample YAML file. For automatizate the tasks of build and deploy on Azure.


<p>&nbsp;</p>

### **Getting Started**

####  1. Configuration: Pre-requisites

This project doesn’t have any dependency and doesn’t need any secret value.

<p>&nbsp;</p>

####  2. Download and installation

Download the repository code with `git clone` or use download button.

We use [Visual Studio 2019](https://docs.microsoft.com/en-GB/visualstudio/ide/quickstart-aspnet-core?view=vs-2022) for working with this project.

<p>&nbsp;</p>

####  3. Latest releases

The latest release of the project deployed in the [Dx29 application](https://dx29.ai/) is: v0.15.01.

<p>&nbsp;</p>

#### 4. API references

>- Diagnosis **with symptoms and genes**: 
>>- Describe: To obtain information on diseases from a list of IDs and in a given language.
>>>- GET request
>>>- URL: ```http://localhost/api/v1/Diagnosis/describe?ids=<List<string>>&lang=<lang>```
>>>- Result: Dictionary with the diseases ids requested as keys, and the value is an object with the information of each disease: Id, Name and Description.
>>- Calculate: To get the list of Dx29 suggested diseases for a patient from his symptoms and/or genes
>>>- POST request
>>>- URL: ```http://localhost/api/v1/Diagnosis/calculate?lang=<lang>&source=<source>&count=<number of results>```
>>>- Body request: DataAnalysis object with the items extracted of the result of the Exomiser execution on the genotype files of the patient (VCFs):
>>>>- symptoms. List of symptoms identifiers (strings).
>>>>- Genes. List of gene items:
>>>>>- Name of the gen
>>>>>- Score of the gen
>>>>>- Combined score
>>>>>- List of diseases identifiers asociated (strings)
>>>- Result: List of objects that contains the information about the diseases results:
>>>>- Id of the disease
>>>>- Name of the disease
>>>>- Description of the disease
>>>>- ScoreDx29, score calculated as total of application Dx29
>>>>- ScoreGenes, score only from genes information
>>>>- ScoreSymptoms, score only from symptoms information
>>>>- Symptoms: The differential diagnosis between disease symptoms and patient symptoms. Is a list of objects with the information of: frecuency of the symptom, if the patient and the disease has it (booleans), if has any related symptom and its relationship.
>>>>- Matches genes. A list of objects with the label of the genes that has de patient and the disease in common.
>>- Phrank: To get the list of Phrank suggested diseases for a patient from his symptoms 
>>>- POST request
>>>- URL: ```http://localhost/api/v1/Diagnosis/phrank?lang=<lang>&source=<source>&count=<number of results>```
>>>- Body request: The same as the previous method (calculate with Dx29 algorithm).
>>>- Results: The same as the previous method (calculate with Dx29 algorithm).
>- Diagnosis **ONLY with symptoms**: 
>>- Wihout path: To get the list of Dx29 suggested diseases for a patient ONLY from his symptoms and with another format results
>>>- POST request
>>>- URL: ```http://localhost/api/v1/Search?skip<int>&count=<int>&lang=<lang>&source=<source> ```
>>>- Body request: The same as the previous methods for calculate with Dx29 algorithm or Phrank algorithm.
>>>- Result: Object with the information about Count, total and BestTotal integers and a list of diseases information objects with:
>>>>- The Id, Name and description of the disease
>>>>- The position, Score (total), phenotype score and genes score values.
>>>>- The differential diagnosis between disease symptoms and patient symptoms. Is a list of objects with the information of the frequency, the Id and the name of the symptom, if has related symptom its Id, name and relationship too, the IC and the Score.

<p>&nbsp;</p>

### **Build and Test**

#### 1. Build

We could use Docker. 

Docker builds images automatically by reading the instructions from a Dockerfile – a text file that contains all commands, in order, needed to build a given image.

>- A Dockerfile adheres to a specific format and set of instructions.
>- A Docker image consists of read-only layers each of which represents a Dockerfile instruction. The layers are stacked and each one is a delta of the changes from the previous layer.

Consult the following links to work with Docker:

>- [Docker Documentation](https://docs.docker.com/reference/)
>- [Docker get-started guide](https://docs.docker.com/get-started/overview/)
>- [Docker Desktop](https://www.docker.com/products/docker-desktop)

The first step is to run docker image build. We pass in . as the only argument to specify that it should build using the current directory. This command looks for a Dockerfile in the current directory and attempts to build a docker image as described in the Dockerfile. 
```docker image build . ```

[Here](https://docs.docker.com/engine/reference/commandline/docker/) you can consult the Docker commands guide.

<p>&nbsp;</p>

#### 2. Deployment

To work locally, it is only necessary to install the project and build it using Visual Studio 2019. 

The deployment of this project in an environment is described in [Dx29 architecture guide](https://dx29-v2.readthedocs.io/en/latest/index.html), in the deployment section. In particular, it describes the steps to execute to work with this project as a microservice (Docker image) available in a kubernetes cluster:

1. Create an Azure container Registry (ACR). A container registry allows you to store and manage container images across all types of Azure deployments. You deploy Docker images from a registry. Firstly, we need access to a registry that is accessible to the Azure Kubernetes Service (AKS) cluster we are creating. For this purpose, we will create an Azure Container Registry (ACR), where we will push images for deployment.
2. Create an Azure Kubernetes cluster (AKS) and configure it for using the prevouos ACR
3. Import image into Azure Container Registry
4. Publish the application with the YAML files that defines the deployment and the service configurations. 

This project includes, in the Deployments folder, YAML examples to perform the deployment tasks as a microservice in an AKS. 

>>- **Interesting link**: [Deploy a Docker container app to Azure Kubernetes Service](https://docs.microsoft.com/en-GB/azure/devops/pipelines/apps/cd/deploy-aks?view=azure-devops&tabs=java)


<p>&nbsp;</p>

### **Contribute**

Please refer to each project's style and contribution guidelines for submitting patches and additions. The project uses [gitflow workflow](https://nvie.com/posts/a-successful-git-branching-model/). 
According to this it has implemented a branch-based system to work with three different environments. Thus, there are two permanent branches in the project:
>- The develop branch to work on the development environment.
>- The master branch to work on the test and production environments.

In general, we follow the "fork-and-pull" Git workflow.

>1. Fork the repo on GitHub
>2. Clone the project to your own machine
>3. Commit changes to your own branch
>4. Push your work back up to your fork
>5. Submit a Pull request so that we can review your changes

The project is licenced under the **(TODO: LICENCE & LINK & Brief explanation)**

<p>&nbsp;</p>
<p>&nbsp;</p>

<div style="border-top: 1px solid !important;
	padding-top: 1% !important;
    padding-right: 1% !important;
    padding-bottom: 0.1% !important;">
	<div align="right">
		<img width="150px" src="https://dx29.ai/assets/img/logo-foundation-twentynine-footer.png">
	</div>
	<div align="right" style="padding-top: 0.5% !important">
		<p align="right">	
			Copyright © 2020
			<a style="color:#009DA0" href="https://www.foundation29.org/" target="_blank"> Foundation29</a>
		</p>
	</div>
<div>
