using System.Security.Cryptography.X509Certificates;

public sealed class Payment
{
    public string Id { get; }
    public PaymentRequest Request { get; }
    public PaymentStatus Status { get; private set; }

    public Payment(string Id, PaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("Id cannot be null or empty", nameof(Id));
        }
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        }

        request.Validate();

        this.Id = Id;
        Request = request;
        Status = PaymentStatus.Processing;

    }

    public void updateStatus(PaymentStatus newStatus)
    {
        Status = newStatus;
    }

    public Payment Snapshot()
    {
        var copy = new Payment(Id, Request);
        copy.updateStatus(Status);
        return copy;
    }
}

