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
						setBuildStatus("Build running", "PENDING");
						sh 'dotnet build'
						setBuildStatus("Build succeeded", "SUCCESS");
					}
					catch(exc){
						setBuildStatus("Build failed", "FAILURE");
						setTestsStatus("Tests skipped", "FAILURE");
					}
				}
			}	
        }
        stage('Test') {
            steps {
                script{
					try{
						setTestsStatus("Tests running", "PENDING");
						sh 'dotnet test --logger:"xunit;LogFilePath=test_result.xml"'
						setTestsStatus("Tests succeeded", "SUCCESS");
					}
					catch(exc){
						setTestsStatus("Tests failed", "FAILURE");
					}
				}
            }
        }
    }
	post {
		always{
            xunit (
                tools: [ xUnitDotNet(pattern: '**/test_result.xml') ]
            )
        }
	}
}