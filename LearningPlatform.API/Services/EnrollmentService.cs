namespace LearningPlatform.API.Services;

public delegate void EnrollmentApprovedHandler(string studentName, string courseTitle);
public delegate void EnrollmentRejectedHandler(string studentName, string courseTitle);

public class EnrollmentService
{
    public event EnrollmentApprovedHandler? OnEnrollmentApproved;
    public event EnrollmentRejectedHandler? OnEnrollmentRejected;

    public EnrollmentService()
    {
        OnEnrollmentApproved += (studentName, courseTitle) =>
            Console.WriteLine($"[Bildirim] {studentName}, '{courseTitle}' kursuna kaydınız onaylandı.");

        OnEnrollmentRejected += (studentName, courseTitle) =>
            Console.WriteLine($"[Bildirim] {studentName}, '{courseTitle}' kursuna kaydınız reddedildi.");
    }

    public void NotifyApproved(string studentName, string courseTitle) =>
        OnEnrollmentApproved?.Invoke(studentName, courseTitle);

    public void NotifyRejected(string studentName, string courseTitle) =>
        OnEnrollmentRejected?.Invoke(studentName, courseTitle);
}
