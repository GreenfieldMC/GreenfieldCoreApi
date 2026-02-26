using GreenfieldCoreServices.Commands.Exceptions;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Commands;

public class ClientCommand(IClientAuthService authService) : BaseCommand("Client command to interact with clients.", "client <help|register|info|list|modify|delete> [options...]")
{
    public override async Task Execute(ILogger<ICommandProcessService> logger, string alias, string[] args, CancellationToken cancellationToken)
    {
        var subCommand = args.GetArg<string>(0)?.ToLower() ?? "help";
        
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (subCommand == "register") await RegisterClient(logger, args.Skip(1).ToArray());
        else if (subCommand == "info") await GetClientInfo(logger, args.Skip(1).ToArray());
        else if (subCommand == "list") await ListClients(logger);
        else if (subCommand == "modify") await ModifyClient(logger, args.Skip(1).ToArray());
        else if (subCommand == "delete") await DeleteClient(logger, args.Skip(1).ToArray());
        else ShowHelp(logger);
    }
    
    private void ShowHelp(ILogger<ICommandProcessService> logger)
    {
        logger.LogInformation("""
                              Client Command Subcommands:
                                help - Show this help message.
                                register <clientName> [roles...] - Register a new client with optional roles, space delimited.
                                info <clientName> - Get information about a specific client.
                                list - List all registered clients.
                                modify <clientName>
                                  roles add|remove <roles...> - Add/remove roles to/from a client, space delimited.
                                  roles clear - Clear all roles from a client.
                                  refresh - Refresh the client secret.
                                  rename <newClientName> - Rename the client.
                                delete <clientName> - Delete a client.
                              """);
    }
    
    private async Task RegisterClient(ILogger<ICommandProcessService> logger, string[] args)
    {
        var clientName = args.GetArg<string>(0);
        if (clientName is null)
            throw new CommandExecutionException("Client name is required. Usage: " + Usage);

        var roles = args.Skip(1).ToList();
        
        var clientTupleResult = await authService.RegisterClient(clientName, roles);
        if (!clientTupleResult.IsSuccessful)
            throw new CommandExecutionException("Failed to register client: " + clientTupleResult.ErrorMessage);
        var clientTuple = clientTupleResult.GetNonNullOrThrow();
        
        logger.LogInformation("Client registered successfully. Copy these details, the secret will not be shown again:\n  Client ID: {ClientId}\n  Client Secret: {ClientSecret}", clientTuple.client.ClientId, clientTuple.secret);
    }
    
    private async Task GetClientInfo(ILogger<ICommandProcessService> logger, string[] args)
    {
        var clientName = args.GetArg<string>(0);
        if (clientName is null)
            throw new CommandExecutionException("Client name is required. Usage: " + Usage);
        
        var foundClientResult = await authService.GetClientByName(clientName);
        if (!foundClientResult.IsSuccessful)
            throw new CommandExecutionException($"Failed to retrieve client '{clientName}': {foundClientResult.ErrorMessage}");
        var foundClient = foundClientResult.GetNonNullOrThrow();
        
        // Log client information in string literal format
        logger.LogInformation("""
                              Client Information:
                                Client ID: {ClientId}
                                Client Name: {ClientName}
                                Created On: {CreatedOn}
                                Roles: {Roles}
                              """, foundClient.ClientId, foundClient.ClientName, foundClient.CreatedOn, string.Join(", ", foundClient.Roles));
    }
    
    private async Task ListClients(ILogger<ICommandProcessService> logger)
    {
        var clientsResult = await authService.GetAllClients();
        if (!clientsResult.IsSuccessful)
            throw new CommandExecutionException("Failed to retrieve clients: " + clientsResult.ErrorMessage);
        var clients = clientsResult.GetNonNullOrThrow().ToList();
        
        if (clients.Count == 0) 
            throw new CommandExecutionException("There are no registered clients.");
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Registered Clients:");
        
        foreach (var client in clients) 
        {
            sb.AppendLine($"  Client Name: {client.ClientName}, Client ID: {client.ClientId}, Created On: {client.CreatedOn}, Roles: {string.Join(", ", client.Roles)}");
        }
        
        logger.LogInformation(sb.ToString());
    }
    
    private async Task ModifyClient(ILogger<ICommandProcessService> logger, string[] args)
    {
        var clientName = args.GetArg<string>(0);
        if (clientName is null)
            throw new CommandExecutionException("Client name is required. Usage: " + Usage);

        var clientResult = await authService.GetClientByName(clientName);
        if (!clientResult.IsSuccessful)
            throw new CommandExecutionException($"Client with name '{clientName}' not found: " + clientResult.ErrorMessage);
        var client = clientResult.GetNonNullOrThrow();
        
        var action = args.GetArg<string>(1)?.ToLower();
        if (action is null)
            throw new CommandExecutionException("Modification action is required. Usage: " + Usage);

        if (action == "refresh")
        {
            var newSecretResult = await authService.RefreshClientSecret(client.ClientId);
            if (!newSecretResult.IsSuccessful)
                throw new CommandExecutionException("Failed to refresh client secret: " + newSecretResult.ErrorMessage);
            var newSecret = newSecretResult.GetNonNullOrThrow();
            logger.LogInformation("Client secret refreshed successfully. New Client Secret: {ClientSecret}", newSecret);
        }
        else if (action == "rename")
        {
            var newClientName = args.GetArg<string>(2);
            if (newClientName is null)
                throw new CommandExecutionException("New client name is required for rename action. Usage: " + Usage);
            
            var updatedClientResult = await authService.UpdateClientName(client.ClientId, newClientName);
            if (!updatedClientResult.IsSuccessful)
                throw new CommandExecutionException("Failed to rename client: " + updatedClientResult.ErrorMessage);
            var updatedClient = updatedClientResult.GetNonNullOrThrow();
            logger.LogInformation("Client renamed successfully. New Client Name: {ClientName}", updatedClient.ClientName);
        }
        else if (action == "roles")
        {
            var roleAction = args.GetArg<string>(2)?.ToLower();
            if (roleAction is null)
                throw new CommandExecutionException("Role modification action is required. Usage: " + Usage);
            
            if (roleAction == "add")
            {
                var rolesToAdd = args.Skip(3).ToList();
                if (rolesToAdd.Count == 0)
                    throw new CommandExecutionException("At least one role is required to add. Usage: " + Usage);
                
                var totalRoles = client.Roles.Union(rolesToAdd).ToList();
                var resultingClientResult = await authService.UpdateClientRoles(client.ClientId, totalRoles);
                if (!resultingClientResult.IsSuccessful)
                    throw new CommandExecutionException("Failed to add roles to client: " + resultingClientResult.ErrorMessage);
                var resultingClient = resultingClientResult.GetNonNullOrThrow();
                
                logger.LogInformation("Roles added to client successfully. Current Roles: {Roles}", string.Join(", ", resultingClient.Roles));
            }
            else if (roleAction == "remove")
            {
                var rolesToRemove = args.Skip(3).ToList();
                if (rolesToRemove.Count == 0)
                    throw new CommandExecutionException("At least one role is required to remove. Usage: " + Usage);
                
                var totalRoles = client.Roles.Except(rolesToRemove).ToList();
                var resultingClientResult = await authService.UpdateClientRoles(client.ClientId, totalRoles);
                if (!resultingClientResult.IsSuccessful)
                    throw new CommandExecutionException("Failed to remove roles from client: " + resultingClientResult.ErrorMessage);
                var resultingClient = resultingClientResult.GetNonNullOrThrow();
                
                logger.LogInformation("Roles removed from client successfully. Current Roles: {Roles}", string.Join(", ", resultingClient.Roles));
            }
            else if (roleAction == "clear")
            {
                var updatedClientResult = await authService.ClearClientRoles(client.ClientId);
                if (!updatedClientResult.IsSuccessful)
                    throw new CommandExecutionException("Failed to clear client roles: " + updatedClientResult.ErrorMessage);
                var updatedClient = updatedClientResult.GetNonNullOrThrow();

                if (updatedClient.Roles.Count > 0)
                    throw new CommandExecutionException("Some, but not all roles were cleared from the client. Roles remaining: " + string.Join(", ", updatedClient.Roles));
                    
                logger.LogInformation("All roles cleared from client successfully.");
            }
            else throw new CommandExecutionException("Invalid role modification action. Usage: " + Usage);
            
        }
        else throw new CommandExecutionException("Invalid modification action. Usage: " + Usage);
    }
    
    private async Task DeleteClient(ILogger<ICommandProcessService> logger, string[] args)
    {
        var clientName = args.GetArg<string>(0);
        if (clientName is null)
            throw new CommandExecutionException("Client name is required. Usage: " + Usage);
        
        var clientResult = await authService.GetClientByName(clientName);
        if (!clientResult.IsSuccessful)
            throw new CommandExecutionException($"Client with name '{clientName}' not found: " + clientResult.ErrorMessage);
        var client = clientResult.GetNonNullOrThrow();
        
        var deletedClientResult = await authService.DeleteClient(client.ClientId);
        if (!deletedClientResult.IsSuccessful)
            throw new CommandExecutionException("Failed to delete client: " + deletedClientResult.ErrorMessage);
        var deletedClient = deletedClientResult.GetNonNullOrThrow();
        
        logger.LogInformation("Client '{ClientName}' deleted successfully.", deletedClient.ClientName);
    }
    
}