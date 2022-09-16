# Clean Architecture Simple Solution Setup

# Some colors for fun :)
YELLOW='\033[1;33m'
BLUE='\033[1;34m'
CYAN='\033[1;36m'
RED='\033[1;31m'
NC='\033[0m'

printf "${BLUE}Clean Architecture Simple Solution Setup\n\n${NC}"

printf "${YELLOW}What is your solution name?\n${NC}"
read SOLUTION_NAME

if [ -z "$SOLUTION_NAME" ]
then
    printf "${RED}Invalid folder name(Null or Empty) :| Try again.${NC}\n"
    exit
fi

printf "${BLUE}Setup for $SOLUTION_NAME ${NC}\n"

mkdir $SOLUTION_NAME
cd $SOLUTION_NAME

printf "${YELLOW}Which Entity Do You Want?${NC} ${CYAN}Enter (1,2 or 3)${NC}\n"

ENTITY_NAME="";

select c in Book Product Song
do
    case $c in
        "Book")
            ENTITY_NAME=$c
            break;;
        "Product")
           ENTITY_NAME=$c
           break;;
        "Song")
           ENTITY_NAME=$c
           break;;
        *)
            ENTITY_NAME="Game"
            printf "${RED}Ooops. Default Entity selected${NC}\n";;
    esac
done

printf "${BLUE}Entity Name is $ENTITY_NAME ${NC}\n\n"

printf "${YELLOW}Creating Solution${NC}\n"
dotnet new sln

mkdir src
cd src

mkdir core infrastructure presentation

cd core
dotnet new classlib -f net6.0 --name $SOLUTION_NAME.Domain
dotnet new classlib -f net6.0 --name $SOLUTION_NAME.Application

cd $SOLUTION_NAME.Application
dotnet add reference ../$SOLUTION_NAME.Domain/$SOLUTION_NAME.Domain.csproj

cd ..
cd ..
cd infrastructure

dotnet new classlib -f net6.0 --name $SOLUTION_NAME.Data
dotnet new classlib -f net6.0 --name $SOLUTION_NAME.Shared

cd $SOLUTION_NAME.Data
dotnet add reference ../../core/$SOLUTION_NAME.Domain/$SOLUTION_NAME.Domain.csproj
dotnet add reference ../../core/$SOLUTION_NAME.Application/$SOLUTION_NAME.Application.csproj

cd ..
cd $SOLUTION_NAME.Shared
mkdir Services
dotnet add reference ../../core/$SOLUTION_NAME.Application/$SOLUTION_NAME.Application.csproj

cd ..
cd ..
cd presentation

dotnet new webapi --name $SOLUTION_NAME.WebApi

cd $SOLUTION_NAME.WebApi

dotnet add reference ../../core/$SOLUTION_NAME.Application/$SOLUTION_NAME.Application.csproj
dotnet add reference ../../infrastructure/$SOLUTION_NAME.Data/$SOLUTION_NAME.Data.csproj
dotnet add reference ../../infrastructure/$SOLUTION_NAME.Shared/$SOLUTION_NAME.Shared.csproj

cd ..
cd ..
cd ..

dotnet sln add src/core/$SOLUTION_NAME.Domain/$SOLUTION_NAME.Domain.csproj
dotnet sln add src/core/$SOLUTION_NAME.Application/$SOLUTION_NAME.Application.csproj
dotnet sln add src/infrastructure/$SOLUTION_NAME.Data/$SOLUTION_NAME.Data.csproj
dotnet sln add src/infrastructure/$SOLUTION_NAME.Shared/$SOLUTION_NAME.Shared.csproj
dotnet sln add src/presentation/$SOLUTION_NAME.WebApi/$SOLUTION_NAME.WebApi.csproj

cd src
cd core
cd $SOLUTION_NAME.Domain
mkdir Entities Enums Settings
cd ..
cd ..

cd presentation
cd $SOLUTION_NAME.WebApi
dotnet add package Microsoft.EntityFrameworkCore.Design

cd ..
cd ..
cd infrastructure
cd $SOLUTION_NAME.Data
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

mkdir Contexts

cd ..
cd ..

cd core
cd $SOLUTION_NAME.Application
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.Extensions.Options.ConfigurationExtensions

mkdir Common
cd Common
mkdir Behaviors Exceptions Interfaces Mappings

cd ..
mkdir Dtos
cd Dtos
mkdir "$ENTITY_NAME"s Email User
cd ..

mkdir "$ENTITY_NAME"s
cd "$ENTITY_NAME"s
mkdir Commands
cd Commands
mkdir Create"$ENTITY_NAME" Update"$ENTITY_NAME" Delete"$ENTITY_NAME"
cd ..

mkdir Queries
cd Queries
mkdir Export"$ENTITY_NAME"s Get"$ENTITY_NAME"s

cd ..
cd ..
cd ..
cd ..
cd presentation

printf "${YELLOW}Creating Vue Client${NC}\n\n"

printf "${BLUE}Guide${NC}\n"
printf "${CYAN}Please pick a preset: -> Manually select features${NC}\n"
printf "${CYAN}Choose Vue Version -> 2.0${NC}\n"
printf "${CYAN}Check the features needed for your project: -> Babel, Router, Vuew, Linter/Formatter${NC}\n"
printf "${CYAN}Choose a version of Vue.js that you want to start the project with -> 2.x${NC}\n"
printf "${CYAN}Use history mode for router? -> YES${NC}\n"
printf "${CYAN}Pick a linter / formatter config -> ESLint + Prettier${NC}\n"
printf "${CYAN}Pick additional lint features: -> Lint on save${NC}\n"
printf "${CYAN}Where do you prefer placing config for Babel, ESLint, etc.? -> In dedicated config files${NC}\n"
printf "${CYAN}Save this as a preset for future projects? -> NO${NC}\n"
printf "${CYAN}Choose a preset -> Vuetify 2 - Vue CLI (recommended)${NC}\n"

sudo npm install -g @vue/cli
npm install -g @vue/cli
vue create web-app
cd web-app
vue add vuetify