using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public NotificationRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Notifications WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<Notification>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Notifications ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<Notification>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<Notification> AddAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Notifications (
                Id, UserId, Type, Title, Message, ActionUrl, 
                IsRead, ReadAt, CreatedAt, ExpiresAt
            ) VALUES (
                @Id, @UserId, @Type, @Title, @Message, @ActionUrl, 
                @IsRead, @ReadAt, @CreatedAt, @ExpiresAt
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notifications SET
                UserId = @UserId,
                Type = @Type,
                Title = @Title,
                Message = @Message,
                ActionUrl = @ActionUrl,
                IsRead = @IsRead,
                ReadAt = @ReadAt,
                ExpiresAt = @ExpiresAt
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Notifications WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM Notifications 
            WHERE UserId = @UserId AND IsRead = 0 
            ORDER BY CreatedAt DESC";
        
        return await _connection.QueryAsync<Notification>(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notifications 
            SET IsRead = 1, ReadAt = GETUTCDATE() 
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = notificationId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DELETE FROM Notifications 
            WHERE ExpiresAt IS NOT NULL AND ExpiresAt < GETUTCDATE()";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }
}
