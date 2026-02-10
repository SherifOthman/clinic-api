using System.Text.Json;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities.Outbox;
using ClinicManagement.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClinicManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly Dictionary<Type, object> _repositories;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _repositories = new Dictionary<Type, object>();
    }

    // Specific repositories using C# 14 field keyword
    public IPatientRepository Patients => field ??= new PatientRepository(_context);
    public IChronicDiseaseRepository ChronicDiseases => field ??= new ChronicDiseaseRepository(_context);
    public ISpecializationRepository Specializations => field ??= new SpecializationRepository(_context);
    public IMedicineRepository Medicines => field ??= new MedicineRepository(_context);
    public IMedicalSupplyRepository MedicalSupplies => field ??= new MedicalSupplyRepository(_context);
    public IMedicalServiceRepository MedicalServices => field ??= new MedicalServiceRepository(_context);
    public IAppointmentRepository Appointments => field ??= new AppointmentRepository(_context);
    public IInvoiceRepository Invoices => field ??= new InvoiceRepository(_context);
    public IPaymentRepository Payments => field ??= new PaymentRepository(_context);
    public IMedicalFileRepository MedicalFiles => field ??= new MedicalFileRepository(_context);
    public IRefreshTokenRepository RefreshTokens => field ??= new RefreshTokenRepository(_context, _dateTimeProvider);

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new BaseRepository<T>(_context);
        }
        
        return (IRepository<T>)_repositories[type];
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get domain events from aggregates before saving
        var domainEvents = GetDomainEvents();
        
        // Save domain events to outbox table
        await SaveDomainEventsToOutboxAsync(domainEvents, cancellationToken);
        
        // Save all changes (including outbox messages) in one transaction
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all domain events from tracked aggregates
    /// </summary>
    private List<IDomainEvent> GetDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        var aggregates = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            domainEvents.AddRange(aggregate.DomainEvents);
            aggregate.ClearDomainEvents();
        }

        return domainEvents;
    }

    /// <summary>
    /// Saves domain events to the outbox table for reliable processing
    /// </summary>
    private async Task SaveDomainEventsToOutboxAsync(
        List<IDomainEvent> domainEvents, 
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            // Cast to DomainEvent to access OccurredOn
            var @event = domainEvent as DomainEvent;
            if (@event == null) continue;

            var outboxMessage = OutboxMessage.Create(
                type: @event.GetType().Name,
                content: JsonSerializer.Serialize(@event, @event.GetType()),
                occurredAt: @event.OccurredOn
            );

            await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
