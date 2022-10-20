void setBuildStatus(String message, String state) {
  step([
      $class: "GitHubCommitStatusSetter",
      reposSource: [$class: "ManuallyEnteredRepositorySource", url: "https://github.com/U7nk/Language"],
      contextSource: [$class: "ManuallyEnteredCommitContextSource", context: "ci/jenkins/build-status"],
      errorHandlers: [[$class: "ChangingBuildStatusErrorHandler", result: "UNSTABLE"]],
      statusResultSource: [ $class: "ConditionalStatusResultSource", results: [[$class: "AnyBuildResult", message: message, state: state]] ]
  ]);
}

void setTestsStatus(String message, String state) {
  step([
      $class: "GitHubCommitStatusSetter",
      reposSource: [$class: "ManuallyEnteredRepositorySource", url: "https://github.com/U7nk/Language"],
      contextSource: [$class: "ManuallyEnteredCommitContextSource", context: "ci/jenkins/tests-status"],
      errorHandlers: [[$class: "ChangingBuildStatusErrorHandler", result: "UNSTABLE"]],
      statusResultSource: [ $class: "ConditionalStatusResultSource", results: [[$class: "AnyBuildResult", message: message, state: state]] ]
  ]);
}
checkout resolveScm(
    source: github(
      repoOwner: 'U7nk',
      repository: 'Language',
      traits: [
        githubSkipNotifications()
      ]
    )
  )
  
pipeline {
    agent {
		docker {
			image 'mcr.microsoft.com/dotnet/sdk:7.0'
		}
	}
	environment {
        HOME = '/tmp'
    }
	
    stages {
        stage('Build') {
			steps{
				script{
					try{
						sh 'dotnet build'
						setBuildStatus("Build succeeded", "SUCCESS");
					}
					catch(exc){
				
					}
				}
			}	
        }
        stage('Test') {
            steps {
                script{
					try{
						sh 'dotnet test --logger:"xunit;LogFilePath=test_result.xml"'
						setTestsStatus("tests succeeded", "SUCCESS");
					}
					catch(exc){
				
					}
				}
            }
        }
        stage('Deploy') {
            steps {
                echo 'Deploying....'
            }
        }
    }
}