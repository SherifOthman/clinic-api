using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class EmailQueueRepository : IEmailQueueRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public EmailQueueRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<EmailQueue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM EmailQueue WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<EmailQueue>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<EmailQueue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM EmailQueue ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<EmailQueue>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<EmailQueue> AddAsync(EmailQueue entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO EmailQueue (
                Id, ToEmail, ToName, Subject, Body, IsHtml, Status, 
                Priority, Attempts, MaxAttempts, SentAt, ErrorMessage, 
                CreatedAt, ScheduledFor
            ) VALUES (
                @Id, @ToEmail, @ToName, @Subject, @Body, @IsHtml, @Status, 
                @Priority, @Attempts, @MaxAttempts, @SentAt, @ErrorMessage, 
                @CreatedAt, @ScheduledFor
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(EmailQueue entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE EmailQueue SET
                ToEmail = @ToEmail,
                ToName = @ToName,
                Subject = @Subject,
                Body = @Body,
                IsHtml = @IsHtml,
                Status = @Status,
                Priority = @Priority,
                Attempts = @Attempts,
                MaxAttempts = @MaxAttempts,
                SentAt = @SentAt,
                ErrorMessage = @ErrorMessage,
                ScheduledFor = @ScheduledFor
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM EmailQueue WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<EmailQueue>> GetPendingEmailsAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP (@BatchSize) * FROM EmailQueue 
            WHERE Status = @PendingStatus 
            AND (ScheduledFor IS NULL OR ScheduledFor <= GETUTCDATE())
            ORDER BY Priority, CreatedAt";
        
        return await _connection.QueryAsync<EmailQueue>(
            new CommandDefinition(sql, new { BatchSize = batchSize, PendingStatus = (int)EmailQueueStatus.Pending }, _transaction, cancellationToken: cancellationToken));
    }
}
