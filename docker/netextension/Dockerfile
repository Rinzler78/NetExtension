FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /workspace

COPY src/Rinzler78.NetExtension.sln src/
COPY src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj src/Rinzler78.NetExtension.Standard/
COPY src/Rinzler78.NetExtension.Tests/Rinzler78.NetExtension.Tests.csproj src/Rinzler78.NetExtension.Tests/
COPY src/Rinzler78.NetExtension.SourceGenerators/Rinzler78.NetExtension.SourceGenerators.csproj src/Rinzler78.NetExtension.SourceGenerators/

RUN dotnet restore src/Rinzler78.NetExtension.sln

FROM restore AS build
WORKDIR /workspace
COPY src/ src/
ARG BUILD_CONFIGURATION=Release
RUN dotnet build src/Rinzler78.NetExtension.sln --configuration ${BUILD_CONFIGURATION}

FROM build AS test
WORKDIR /workspace
RUN mkdir -p /results
ENTRYPOINT ["/bin/sh", "-lc"]

FROM build AS pack
WORKDIR /workspace
RUN mkdir -p /artifacts && dotnet pack src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj --configuration Release --no-build --output /artifacts

CMD ["/bin/sh"]
