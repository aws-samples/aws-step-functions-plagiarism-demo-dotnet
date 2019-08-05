# AWS Step Functions Plagiarism Demo - Test Centre

An app designed to test students as part of an AWS Step Functions workflow.

It can accept GET variables to refer to an exam attempt: IncidentId, ExamId,
Test Centre.

![TestCentre Screenshot](testcentre-screenshot.png)

## Test Centre role in Plagiarism Step Functions workflow

The flow for use of the Test Centre is:

- A Step Functions execution is initiated.
- An exam attempt is generated.
- A mail is sent to the student, with a link in the following format:

`GET https://testcentre.example.com/?ExamID=foo&IncidentId=bar&TaskToken=baz`

- A student then completes a test. Once done, a request is sent back to Step Functions as a POST request, to an API Gateway endpoint, with their score and the task token:

`POST https://1234567890-abcdefgh.amazonaws.com/submitExam`

**Payload**
```
{
  "ExamId": "food",
  "IncidentId": "bar",
  "TaskToken": "baz",
  "Score": "69"
}
```

# Deploying

1. This project is designed to deploy via the Amplify Console.

2. It assumes you have already deployed the parent `AWSStepFunctionsPlagiarismDemo.`

3. Get the API Gateway endpoint after it is deployed:

`aws cloudformation describe-stacks --stack-name aws-step-functions-plagiarism-demo --query 'Stacks[].Outputs' | grep api`

4. Then, create a new Amplify Console project.

5. Connect it to a repository hosting this code, then set an enviromnet variable called `APIGW_ENDPOINT` with the value that you just got from the CloudFormation output.

6. Deploy the site.

## Project setup

This project was built with the `vue-cli`, and inherits `yarn` as a package
manager.

You will likely need the `vue-cli` package installed globally.

Then you can run the following.

### Install dependencies

```
yarn install
```

### Compile and hot-reloads for development
```
yarn run serve
```

### Compile and minify for production
```
yarn run build
```

### Run tests
```
yarn run test
```

### Lint and fix files
```
yarn run lint
```

### Customize configuration
See [Configuration Reference](https://cli.vuejs.org/config/).
