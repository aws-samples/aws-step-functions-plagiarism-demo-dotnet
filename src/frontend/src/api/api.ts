const API_ENDPOINT = process.env.NEXT_PUBLIC_API_ENDPOINT;

export interface ExamData {
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
    console.log(process.env);
    console.log(API_ENDPOINT);
    const response = await fetch(`${API_ENDPOINT}/exam`, { method: 'POST', mode: 'cors', body: JSON.stringify(examData) });
    if (!response.ok) throw (response);
    return await response.json();
}

export interface Incident {
    StudentId: string,
    IncidentDate: string
}

export interface StepFunctionInfo {
    executionArn: string
    startDate: string
}

export async function createIncident(incidentData: Incident): Promise<StepFunctionInfo> {
    //return { "executionArn": "asdf", "startDate": "tomorrow" };
    const response = await fetch(`${API_ENDPOINT}/incident`, { method: 'POST', mode: 'cors', headers: { "Content-Type": "application/json" }, body: JSON.stringify(incidentData) });
    const { executionArn, startDate } = await response.json();
    return { executionArn, startDate };
}

