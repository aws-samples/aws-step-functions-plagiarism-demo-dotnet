'use client'

import { useEffect, useState } from 'react'
import { ExamData, submitExam } from '@/api/api'
import { useSearchParams } from 'next/navigation';

type ExamIntegrationProps = {
    setExamSubmitted: (isExamSubmitted: boolean) => void
    score: number
}

export default function ExamIntegration({ score, setExamSubmitted }: ExamIntegrationProps) {
    const [examData, setExamData] = useState<ExamData>({
        // Exam score (out of 100).
        Score: score,
        // Unique identifier for plagiarism incident.
        IncidentId: 'Not supplied',
        // Unique identifier for exam attempt.
        ExamId: 'Not supplied',
        // Task Token unique to the current Step Functions execution.
        TaskToken: 'Not supplied'
    });
    const [isSubmitSuccessful, setIsSubmitSuccessful] = useState(false);
    const [submitMessage, setSubmitMessage] = useState('');

    const params = useSearchParams();

    useEffect(() => {
        // If present, get the primary keys, then set our hidden form value.
        // Keys can get passed as a GET variables,
        // in the form ?/TaskToken=baz&IncidentId=foo&ExamId=bar
        const incidentId = params.get('IncidentId') || 'Not supplied';
        const examId = params.get('ExamId') || 'Not supplied';
        const taskToken = params.get('TaskToken')?.replaceAll(" ", "+") || 'Not supplied';
        setExamData((examData) => ({ ...examData, IncidentId: incidentId, ExamId: examId, TaskToken: taskToken }));
    }, [params]);

    // Add this useEffect to update the score
    useEffect(() => {
        setExamData((examData) => ({ ...examData, Score: score }));
    }, [score]);

    function submitToStepFunctions(event: any) {
        event.preventDefault();
        // Post our response back to Step Functions to continue the flow.
        submitExam(examData).then(response => {
            setIsSubmitSuccessful(true);
            console.log(response);
            setExamSubmitted(true);
            setSubmitMessage('Your exam has been submitted successfully.');
        }).catch(error => {
            // Something went wrong.
            setIsSubmitSuccessful(true);
            setSubmitMessage(error.message || 'Your exam has been submitted successfully, with errors.');
            console.error('Error submitting exam:', error);
        });
    }

    return (
        <div id="exam-integration">
            <div className="box">
                <div className="content">
                    <h1 className="subtitle is-4">Test Step Functions Integration</h1>
                    <div id="meta" className="field is-grouped is-grouped-multiline">
                        <div className="control">
                            <div className="tags has-addons">
                                <span className="tag">Incident ID</span>
                                <span className="tag is-info">{examData.IncidentId}</span>
                            </div>
                        </div>
                        <div className="control">
                            <div className="tags has-addons">
                                <span className="tag">Exam ID</span>
                                <span className="tag is-warning">{examData.ExamId}</span>
                            </div>
                        </div>
                        <div className="control">
                            <div className="tags has-addons">
                                <span className="tag">Task Token</span>
                                {/* <!--- TODO: improve this UX --> */}
                                <span id="task-token-preview" className="tag is-danger">{examData.TaskToken}</span>
                            </div>
                        </div>
                        <hr />
                    </div>
                    <p>
                        This application expects three GET variables to be passed in the URL:       </p>
                    <ul>
                        <li><span className="is-family-monospace">IncidentId</span> - the Incident this attempt relates to.</li>
                        <li><span className="is-family-monospace">ExamId</span> - the unique identifier for this particular attempt.</li>
                        <li><span className="is-family-monospace">TaskToken</span> - a callback task token for this Step Function execution.</li>
                    </ul>

                    <p>
                        You can click the button below to pass some dummy variables, to check the integration is working.
                    </p>
                </div>

                <div className="level">
                    <div className="level-left">
                        <div className="control level-item">
                            <a className="btn btn-info" href="?IncidentId=foo123&ExamId=bar456&TaskToken=AAAAKgAAAAIAAAAAAAAAASWo0UxfuXhTgRdqc1rySg1PZc/6rKbRHisQGn/pi89D8fIVujH6v3RS2DnWLIyPkgfJnI0H+WkRFoz8wXhfEgVvz6xRHEPURX2oi/eMINBFbGev89Kydex7Q3fi0mELPrD/gyfKsKwwsPPjXRPj1G4sDXwP3tZYdKGD/f1GfR04IIUB1uAKHSu6zTeQglzm+6cTbxAcGqcBgg6aIcUCY2PoA0Rk+mLzu/Y7+rY0FwQQCl0Npb8R8VLzEC2guM67oDFd9pKWbhEDkB700wNVQvpYAr6bvHFlbpVtXwoD16T8rZ0c802nvCEl7MvW/t+zxBSxbidGWsvXhF172iWpWcPnsEPUBjrcDglf/6MgRxzNIc4VXmRRp/GYBrEnYU1qf0qkeOQb0Fuq30nxtOvRcYp+WwFOMNXDpezhgu4LGzYT2J69+0mljvRRGNMXt0hG52fysjL53j4IMxZF/Hqy3ZW+DlkbFW38ePZufuBwsuT2oUbc1IR3+W5pZZ0OAuJLWDjHQRAsDVEaVQyjsHMynm13wFYPUvFXUorORLX9Y8fI8Fphp71ZDMPvbtqAhfc4Z25x08abQXSMKoXsmwNAaQssvyT42qXO79UnlfbsgALavU6Q2oLgUSmAX0Ou9lhgIVYhHfZDvX9lGTnlRmFdhz4#exam-integration">
                                Test GET variables</a>
                        </div>

                        <div className="control level-item" onClick={(e) => submitToStepFunctions(e)}>
                            <a className="btn btn-dark" href="#">Submit to Step Function Execution</a>
                        </div>
                    </div>
                </div>

                {submitMessage.length > 0 && (
                    isSubmitSuccessful ? (
                        <div className="notification success exam-submitted is-primary">
                            <button onClick={() => setSubmitMessage('')} className="delete"></button>
                            Your exam has been submitted. Your score was <strong>{examData.Score}%</strong> Your assessor will be in touch to let you
                            know what the next steps are.
                        </div>
                    ) : (
                        <div className="notification failure submission-failed is-error">
                            <button onClick={() => setSubmitMessage('')} className="delete"></button>
                            <p>There was an error when submitting your exam:</p>
                            <br />
                            <pre>{submitMessage}</pre>
                        </div>
                    )
                )}
            </div>
        </div>

    );
}


