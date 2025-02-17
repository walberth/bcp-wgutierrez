FROM mcr.microsoft.com/mssql/server:2019-latest

USER root

# Install GnuPG, curl, and other necessary tools
RUN apt-get update && apt-get install -y gnupg2 curl apt-transport-https

# Install curl and other dependencies
RUN apt-get install -y curl apt-transport-https

# Add Microsoft's GPG key
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -

# Add the Microsoft repository
RUN curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list

# Update the package list again
RUN apt-get update

# Install SQL Server command-line tools
RUN ACCEPT_EULA=Y apt-get install -y msodbcsql17 mssql-tools unixodbc-dev

# Add tools to PATH
RUN echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc

# Copy the init-delay.sh script into the container
COPY --chmod=755 init-delay.sh /usr/local/bin/init-delay.sh

# Ensure the script has executable permissions
# RUN chmod +x /usr/local/bin/init-delay.sh

# Clean up
RUN apt-get clean && rm -rf /var/lib/apt/lists/*

# Set back to the default user for running SQL Server
USER mssql

EXPOSE 1433

CMD /opt/mssql/bin/sqlservr
