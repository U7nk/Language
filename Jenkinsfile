pipeline {
    agent any

    stages {
        stage('Build') {
            steps {
				try{
					echo 'Building..'
					setBuildStatus("Build succeeded", "SUCCESS");
				}
				catch(exc){
				
				}
            }
        }
        stage('Test') {
            steps {
                echo 'Testing..'
            }
        }
        stage('Deploy') {
            steps {
                echo 'Deploying....'
            }
        }
    }
}