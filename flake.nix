{
  description = "OW3N";

  inputs = {
    nixpkgs = {
      url = "github:nixos/nixpkgs/nixos-unstable";
    };
    flake-utils = {
      url = "github:numtide/flake-utils";
    };
  };
  outputs =
    {
      nixpkgs,
      flake-utils,
      self,
      ...
    }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs {
          inherit system;
        };

        dotnetPkg = pkgs.dotnetCorePackages.sdk_9_0;

        buildDeps = with pkgs; [
          nodejs
          dotnetPkg
          zlib
          zlib.dev
          openssl
        ];

        projectFile = "Ordis.csproj";
        projectName = "OW3N";
        nugetDeps = ./deps.json;
        description = "Flexible D&D app";
        version = "1.0";
      in
      {

        packages = {
          default = pkgs.buildDotnetModule {
            dotnet-sdk = dotnetPkg;
            pname = projectName;
            version = version;
            src = ./.;
            projectFile = projectFile;
            nugetDeps = nugetDeps;
            meta = {
              description = description;
              license = pkgs.lib.licenses.mit;
            };
          };
        };

        devShell = pkgs.mkShell {
          buildInputs =
            with pkgs;
            [

              (pkgs.writeShellScriptBin "compileSass" (''sass sass/app.scss wwwroot/app.css''))
              (pkgs.writeShellScriptBin "watchSass" (''sass --watch sass/app.scss:wwwroot/app.css ''))

              (pkgs.writeShellScriptBin "publishAndRun" (
                # bash
                ''
                  dotnet publish -c Release
                  export $(grep -v '^#' .env | xargs)
                  cp .env bin/Release/net9.0/publish/
                  cd bin/Release/net9.0/publish/
                  dotnet Ordis.dll
                ''))

              (pkgs.writeShellScriptBin "run" (
                # bash
                ''
                  export $(grep -v '^#' .env | xargs)

                  sass --watch sass/app.scss:wwwroot/app.css &
                  p1=$!

                  cleanup() {
                      kid=$(pgrep -P "$p1")
                      kill "$kid" "$p1" 2>/dev/null
                  }
                  trap cleanup EXIT
                  trap cleanup INT

                  dotnet watch run
                ''))

              (pkgs.writeShellScriptBin "updateDatabase" (
                # bash
                ''
                  export $(grep -v '^#' .env | xargs)
                  dotnet tool restore
                  dotnet ef database update
                ''))

              (pkgs.writeShellApplication {
                name = "updateDeps";

                runtimeInputs = with pkgs; [
                  dotnetPkg
                  nuget-to-json

                ];

                text = ''
                  dotnet restore --packages=packageDir                     
                  nuget-to-json packageDir > deps.json
                '';
              })

              (pkgs.writeShellScriptBin "initDatabase" (
                # bash
                ''
                  #!/usr/bin/env bash

                  if ! command -v psql >/dev/null 2>&1; then
                    echo "psql no here. Need PostgreSQL."
                    exit 1
                  fi

                  read -r -s -p "Enter password for ow3n: " PW
                  echo

                  sudo -u postgres psql -v pw="$PW" -f initDb.sql

                  echo "connectionstrings__characterdb=\"host=localhost;username=ow3n;password=$PW;database=ow3n\"" >> .env

                  updateDatabase

                ''))

              netcoredbg
              bruno
              omnisharp-roslyn
              dart-sass
            ]
            ++ buildDeps;

          shellHook = ''
            DOTNET_ROOT="${dotnetPkg}"
            DOTNET_SYSTEM_GLOBALIZATION_INVARIANT="1"

            DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=1
          '';

        };
      }
    );
}
