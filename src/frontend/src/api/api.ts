const API_ENDPOINT = process.env.API_ENDPOINT;

export interface Incident {
    StudentId: string,
    IncidentDate?: Date
}

export interface StepFunctionInfo {
    executionArn: string
    startDate: string
}

export async function createIncident(incidentData: Incident): Promise<StepFunctionInfo> {
    return { "executionArn": "asdf", "startDate": "tomorrow" };
    const response = await fetch(`${API_ENDPOINT}incident`, { method: 'POST', body: JSON.stringify(incidentData) });
    const { executionArn, startDate } = await response.json();
    return { executionArn, startDate };
}

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
    return;
    const response = await fetch(`${API_ENDPOINT}exam`, { method: 'POST', body: JSON.stringify(examData) });
    if (!response.ok) throw(response);
    return response;
}

