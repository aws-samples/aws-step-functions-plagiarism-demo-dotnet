'use client'

import { useState } from 'react'
import { Incident, createIncident } from '@/api/api';



export default function AdminPage({ }) {
    const [incidentData, setIncidentData] = useState<Incident>({ StudentId: '', IncidentDate: undefined });
    const [showMessage, setShowMessage] = useState(false);
    const [executionArn, setExecutionArn] = useState('');
    const [executionStartDate, setExecutionStartDate] = useState();


    function resetForm() {
        setIncidentData({ ...incidentData, StudentId: '', IncidentDate: undefined });
        setShowMessage(false);
    }

    async function incidentFormSubmitted() {
        const response = await createIncident(incidentData);
        const responseJson = await response.json();

        fetch('https://[ADD YOUR API GATEWAY URL HERE]/incident', { method: 'POST', body: JSON.stringify(incidentData) })
            .then(response => response.json())
            .then(response => {
                // get status
                setExecutionArn(response?.body?.executionArn)
                setExecutionStartDate(response?.body?.startDate);
                setShowMessage(true);

            }, error => {
                console.log(error)

            });
    }


    return (
        <div className="container text-white my-5">
            <div className="row">
                <div className="col">
                    <div className="jumbotron pt-4 pb-4">
                        <img src="/images/AWS_logo_RGB_REV.png" title="AWS" alt="AWS" width="160" height='100' className="mx-auto d-block" />
                        <blockquote className="blockquote text-center">
                            <h1 className="display-4 pt-4">Developing with <img src="/images/header-icon_step-functions.png" title="AWS" alt="AWS" className="mx-auto" /> AWS Step Functions</h1>
                        </blockquote>
                    </div>
                </div>
            </div>
            <div className="row">
                <div className="col">
                    <div className="row">
                        <div className="col">
                            <form id="incidentForm" onSubmit={(e) => incidentFormSubmitted()}>
                                <h3 className="text-primary">Create new plagiarism incident</h3>
                                <div className="form-group">
                                    <label htmlFor="StudentId">Student ID</label>
                                    <input type="text" className="form-control col-8" id="StudentId" v-model="incidentData.StudentId" />
                                </div>
                                <div className="form-group">
                                    <label htmlFor="IncidentDate">Incident Date</label>
                                    <input type="date" className="form-control col-8" id="IncidentDate" v-model="incidentData.IncidentDate" />
                                </div>
                                <div className="form-group">
                                    <button type="submit" className="btn btn-primary">Create incident</button>
                                    <button type="button" className="btn btn-secondary ml-2" onClick={resetForm}>New incident</button>
                                </div>
                            </form>
                        </div>
                    </div>
                    <div>
                        {showMessage && (
                            <>
                                <div className="alert alert-success" role="alert" v-show="showMessage">
                                    Created new incident for Student ID: {incidentData?.StudentId}<br />
                                </div>
                                <div className="alert alert-info small" role="alert" v-show="showMessage">
                                    <strong>Execution ARN:</strong> <a href={'https://ap-southeast-2.console.aws.amazon.com/states/home?region=ap-southeast-2#/executions/details/' + executionArn}>{executionArn}</a><br />
                                    <strong>Start Date:</strong> {executionStartDate}
                                </div>
                            </>
                        )}
                    </div>

                </div>
                <div className="col">
                    <div className="card bg-secondary ">
                        <div className="card-header text-white">SCENARIO</div>
                        <div className="card-body text-white">
                            <p>University students caught plagiarising on exams and assignments are required to take exams
                                to test their knowledge of the universities referencing standard. Students get three attempts to pass the exam
                                before administrative action is taken.</p>

                            <p>This demo uses exposes an <a href="https://aws.amazon.com/step-functions/">AWS Step Function</a>  via an <a href="https://aws.amazon.com/api-gateway/">Amazon API Gateway</a>. The step-function definition invokes
                                tasks via <a href="https://aws.amazon.com/lambda/">AWS Lambda</a> (Python 3.6), that store results in <a href="https://aws.amazon.com/dynamodb">Amazon DynamoDB</a>. Notifications are implemented
                                via <a href="https://aws.amazon.com/dynamodb">Amazon SNS</a> and <a href="https://aws.amazon.com/xray/">AWS X-Ray</a> provides distributed tracing capability.</p>
                        </div>
                    </div>
                    <img src="/images/stepfunction.png" className="img-fluid pt-5" alt="state-machine" />

                </div>
            </div>




        </div>
    )
}



