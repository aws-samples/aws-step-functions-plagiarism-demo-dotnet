export type Incident = {
    StudentId: string,
    IncidentDate?: Date
}

export async function createIncident(incidentData: Incident) {
    return await fetch('https://[ADD YOUR API GATEWAY URL HERE]/incident', { method: 'POST', body: JSON.stringify(incidentData) })
}


export type ExamData = {
    // Exam score (out of 100).
    Score: number,
    // Unique identifier for plagiarism incident.
    IncidentId: string,
    // Unique identifier for exam attempt.
    ExamId: string,
    // Task Token unique to the current Step Functions execution.
    TaskToken: string
}

export async function submitExam(examData: ExamData) {
    let apiName = 'PlagiarismStepFunctionsDemo';
    let path = '/exam';
    // TODO: use Amplify library?
    return await fetch('https://[ADD YOUR API GATEWAY URL HERE]/incident', { method: 'POST', body: JSON.stringify(examData) })
}

