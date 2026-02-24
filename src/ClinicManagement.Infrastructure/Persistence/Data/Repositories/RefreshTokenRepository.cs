using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicManagement.Infrastructure.Persistence.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction? _transaction;

    public RefreshTokenRepository(SqlConnection connection, SqlTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM RefreshTokens WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<RefreshToken>(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM RefreshTokens WHERE Token = @Token";
        return await _connection.QueryFirstOrDefaultAsync<RefreshToken>(
            new CommandDefinition(sql, new { Token = token }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM RefreshTokens 
            WHERE UserId = @UserId 
            AND IsRevoked = 0 
            AND ExpiryTime > GETUTCDATE()";
        
        return await _connection.QueryAsync<RefreshToken>(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE RefreshTokens 
            SET IsRevoked = 1, RevokedAt = GETUTCDATE()
            WHERE UserId = @UserId AND IsRevoked = 0";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DELETE FROM RefreshTokens 
            WHERE IsRevoked = 1 OR ExpiryTime <= GETUTCDATE()";
        
        return await _connection.ExecuteAsync(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM RefreshTokens";
        return await _connection.QueryAsync<RefreshToken>(
            new CommandDefinition(sql, transaction: _transaction, cancellationToken: cancellationToken));
    }

    public async Task<RefreshToken> AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO RefreshTokens (
                Id, UserId, Token, ExpiryTime, CreatedByIp, 
                IsRevoked, RevokedAt, RevokedByIp, ReplacedByToken
            ) VALUES (
                @Id, @UserId, @Token, @ExpiryTime, @CreatedByIp,
                @IsRevoked, @RevokedAt, @RevokedByIp, @ReplacedByToken
            )";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
        
        return entity;
    }

    public async Task UpdateAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE RefreshTokens SET
                Token = @Token,
                ExpiryTime = @ExpiryTime,
                IsRevoked = @IsRevoked,
                RevokedAt = @RevokedAt,
                RevokedByIp = @RevokedByIp,
                ReplacedByToken = @ReplacedByToken
            WHERE Id = @Id";
        
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, entity, _transaction, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM RefreshTokens WHERE Id = @Id";
        await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _transaction, cancellationToken: cancellationToken));
    }
}
