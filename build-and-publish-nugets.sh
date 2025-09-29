#!/bin/bash

# CSharpEssentials NuGet Package Build and Publish Script
# Author: SenRecep
# Version: 2.0
# Usage: ./build-and-publish-nugets.sh [OPTIONS] [API_KEY]
# Options: --parallel, --verbose, --dry-run, --skip-build
# Run from the main solution directory (where CSharpEssentials.slnx is located)

set -euo pipefail  # Exit on any error, undefined variable, or pipe failure

# Global configuration
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly NUPKGS_DIR="$SCRIPT_DIR/nupkgs"
readonly NUGET_SOURCE="https://api.nuget.org/v3/index.json"
readonly BUILD_CONFIG="Release"

# Colors for better output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[0;33m'
readonly BLUE='\033[0;34m'
readonly PURPLE='\033[0;35m'
readonly CYAN='\033[0;36m'
readonly NC='\033[0m' # No Color

# Options
PARALLEL_BUILD=false
VERBOSE=false
DRY_RUN=false
SKIP_BUILD=false
FORCE_PACK=false
API_KEY=""

# Logging functions
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}" >&2
}

log_step() {
    echo -e "${PURPLE}� $1${NC}"
}

log_progress() {
    echo -e "${CYAN}📦 $1${NC}"
}

# Error handling
error_exit() {
    log_error "$1"
    exit "${2:-1}"
}

# Cleanup function
cleanup() {
    local exit_code=$?
    if [[ $exit_code -ne 0 ]]; then
        log_error "Script failed with exit code $exit_code"
    fi
    exit $exit_code
}

# Set up trap for cleanup
trap cleanup EXIT

# Parse command line arguments
parse_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --parallel)
                PARALLEL_BUILD=true
                shift
                ;;
            --verbose)
                VERBOSE=true
                shift
                ;;
            --dry-run)
                DRY_RUN=true
                shift
                ;;
            --skip-build)
                SKIP_BUILD=true
                shift
                ;;
            --force-pack)
                FORCE_PACK=true
                shift
                ;;
            --help|-h)
                show_help
                exit 0
                ;;
            *)
                if [[ -z "$API_KEY" ]]; then
                    API_KEY="$1"
                else
                    error_exit "Unknown argument: $1"
                fi
                shift
                ;;
        esac
    done
}

show_help() {
    cat << EOF
CSharpEssentials NuGet Package Build and Publish Script

Usage: $0 [OPTIONS] [API_KEY]

Options:
  --parallel      Enable parallel processing for same-level packages
  --verbose       Enable verbose logging
  --dry-run       Show what would be done without actually doing it
  --skip-build    Skip the build step and only package existing builds
  --force-pack    Force packaging even with NU5017 warnings
  --help, -h      Show this help message

Examples:
  $0                                    # Build and package only
  $0 YOUR_API_KEY                       # Build, package, and publish
  $0 --parallel YOUR_API_KEY            # Use parallel processing
  $0 --dry-run --verbose YOUR_API_KEY   # Show what would happen
  $0 --force-pack YOUR_API_KEY          # Force pack even with warnings

EOF
}

# Validation functions
validate_environment() {
    log_step "Validating environment..."
    
    # Check if we're in the correct directory
    if [[ ! -f "CSharpEssentials.slnx" ]]; then
        error_exit "CSharpEssentials.slnx not found. Please run this script from the solution root directory."
    fi
    
    # Check required tools
    if ! command -v dotnet &> /dev/null; then
        error_exit ".NET CLI (dotnet) is not installed or not in PATH"
    fi
    
    # Check .NET version
    local dotnet_version
    dotnet_version=$(dotnet --version)
    log_info ".NET CLI version: $dotnet_version"
    
    log_success "Environment validation completed"
}

# Clean and prepare output directory
prepare_output_directory() {
    log_step "Preparing output directory..."
    
    if [[ -d "$NUPKGS_DIR" ]]; then
        if [[ "$DRY_RUN" == "true" ]]; then
            log_info "Would remove existing nupkgs directory"
        else
            rm -rf "$NUPKGS_DIR"
            log_info "Removed existing nupkgs directory"
        fi
    fi
    
    if [[ "$DRY_RUN" == "false" ]]; then
        mkdir -p "$NUPKGS_DIR"
        log_success "Created nupkgs directory: $NUPKGS_DIR"
    else
        log_info "Would create nupkgs directory: $NUPKGS_DIR"
    fi
}

# Build and packaging functions
build_solution() {
    if [[ "$SKIP_BUILD" == "true" ]]; then
        log_info "Skipping build as requested"
        return 0
    fi
    
    log_step "Building solution..."
    
    local build_cmd="dotnet build -c $BUILD_CONFIG"
    if [[ "$VERBOSE" == "true" ]]; then
        build_cmd="$build_cmd --verbosity normal"
    else
        build_cmd="$build_cmd --verbosity minimal"
    fi
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "Would run: $build_cmd"
        return 0
    fi
    
    if $build_cmd; then
        log_success "Build completed successfully"
    else
        error_exit "Build failed!"
    fi
}

# Execute command with proper logging and dry-run support
execute_command() {
    local cmd="$1"
    local description="${2:-$cmd}"
    
    if [[ "$VERBOSE" == "true" ]]; then
        log_info "Executing: $cmd"
    fi
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "Would execute: $description"
        return 0
    fi
    
    # Capture both stdout and stderr
    local output
    local exit_code
    
    output=$(eval "$cmd" 2>&1)
    exit_code=$?
    
    # Special handling for packaging commands with NU5017
    if [[ $exit_code -ne 0 && "$description" =~ "Packaging" ]]; then
        # Check if package was actually created despite NU5017 error
        local project_name=$(echo "$description" | sed 's/Packaging //')
        if ls "$NUPKGS_DIR"/${project_name}.*.nupkg 1> /dev/null 2>&1; then
            if [[ "$FORCE_PACK" == "true" ]]; then
                log_info "Package created despite NU5017 warning: $project_name"
                return 0
            fi
        fi
    fi
    
    if [[ $exit_code -eq 0 ]]; then
        if [[ "$VERBOSE" == "true" && -n "$output" ]]; then
            echo "$output"
        fi
        return 0
    else
        log_error "Command failed: $description"
        if [[ -n "$output" ]]; then
            echo "$output" | head -20  # Show first 20 lines of error output
        fi
        return $exit_code
    fi
}

# Package a single project
package_project() {
    local project_path="$1"
    local project_name
    project_name=$(basename "$project_path" .csproj)
    
    local pack_cmd="dotnet pack '$project_path' -c $BUILD_CONFIG -o '$NUPKGS_DIR'"
    
    # Add properties to ensure package content is included
    pack_cmd="$pack_cmd -p:IsPackable=true -p:IncludeBuildOutput=true -p:GeneratePackageOnBuild=false"
    
    # Force packaging even if there are warnings
    if [[ "$FORCE_PACK" == "true" ]]; then
        pack_cmd="$pack_cmd --no-restore -p:NoWarn=NU5017"
    fi
    
    if [[ "$VERBOSE" == "true" ]]; then
        pack_cmd="$pack_cmd --verbosity normal"
    else
        pack_cmd="$pack_cmd --verbosity minimal"
    fi
    
    execute_command "$pack_cmd" "Packaging $project_name"
}

# Package projects in parallel if enabled
package_projects_batch() {
    local projects=("$@")
    local pids=()
    local failed_projects=()
    local success_count=0
    
    if [[ "$PARALLEL_BUILD" == "true" && ${#projects[@]} -gt 1 ]]; then
        log_info "Packaging ${#projects[@]} projects in parallel..."
        
        for project in "${projects[@]}"; do
            if [[ "$DRY_RUN" == "false" ]]; then
                package_project "$project" &
                pids+=($!)
            else
                package_project "$project"
            fi
        done
        
        # Wait for all background processes to complete
        if [[ "$DRY_RUN" == "false" ]]; then
            for i in "${!pids[@]}"; do
                if wait "${pids[$i]}"; then
                    ((success_count++))
                else
                    failed_projects+=("${projects[$i]}")
                fi
            done
        fi
    else
        log_info "Packaging ${#projects[@]} projects sequentially..."
        for project in "${projects[@]}"; do
            if package_project "$project"; then
                ((success_count++))
            else
                failed_projects+=("$project")
            fi
        done
    fi
    
    # Report results
    log_info "Packaging results: $success_count successful, ${#failed_projects[@]} failed"
    
    if [[ ${#failed_projects[@]} -gt 0 ]]; then
        log_warning "Failed to package the following projects:"
        printf '%s\n' "${failed_projects[@]}"
        
        # Don't fail immediately, just warn and continue
        log_warning "Continuing with successful packages..."
        return 0
    fi
    
    return 0
}

# Define project dependencies in levels
declare -A PROJECT_LEVELS
PROJECT_LEVELS=(
    [0]="CSharpEssentials.Core CSharpEssentials.Enums CSharpEssentials.Time CSharpEssentials.Clone CSharpEssentials.AspNetCore CSharpEssentials.GcpSecretManager CSharpEssentials.RequestResponseLogging"
    [1]="CSharpEssentials.Errors CSharpEssentials.Json"
    [2]="CSharpEssentials.Results"
    [3]="CSharpEssentials.Any CSharpEssentials.Maybe CSharpEssentials.Rules CSharpEssentials.Entity CSharpEssentials.EntityFrameworkCore"
    [4]="CSharpEssentials"
)

# Package all projects in dependency order
package_all_projects() {
    log_step "Packaging projects in dependency order..."
    
    local max_level=4
    local total_success=0
    local total_failed=0
    
    for level in $(seq 0 $max_level); do
        local projects_str="${PROJECT_LEVELS[$level]}"
        if [[ -n "$projects_str" ]]; then
            IFS=' ' read -ra projects <<< "$projects_str"
            local project_paths=()
            
            log_progress "Level $level: Processing ${projects[*]}"
            
            for project in "${projects[@]}"; do
                local project_path="$project/$project.csproj"
                if [[ -f "$project_path" ]]; then
                    project_paths+=("$project_path")
                else
                    log_warning "Project file not found: $project_path"
                fi
            done
            
            if [[ ${#project_paths[@]} -gt 0 ]]; then
                # Don't exit on failure, continue with next level
                if package_projects_batch "${project_paths[@]}"; then
                    log_success "Level $level packaging completed successfully"
                else
                    log_warning "Level $level packaging completed with some failures"
                fi
            fi
        fi
    done
    
    log_info "Packaging phase completed. Check individual project status above."
}

# List generated packages
list_packages() {
    log_step "Listing generated packages..."
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_info "Would list packages in: $NUPKGS_DIR"
        return 0
    fi
    
    if [[ ! -d "$NUPKGS_DIR" ]]; then
        log_warning "Package directory does not exist: $NUPKGS_DIR"
        return 1
    fi
    
    local packages
    packages=$(find "$NUPKGS_DIR" -name "*.nupkg" -type f 2>/dev/null)
    
    if [[ -n "$packages" ]]; then
        echo ""
        log_success "Generated packages:"
        echo "$packages" | sed 's|.*/||' | sort
        echo ""
        
        local count
        count=$(echo "$packages" | wc -l)
        log_info "Total packages: $count"
        
        if [[ $count -eq 0 ]]; then
            log_warning "No packages were successfully created. Check the errors above."
            log_info "Common solutions:"
            log_info "  - Ensure project files have proper package metadata"
            log_info "  - Check if projects contain actual code/content"
            log_info "  - Verify all referenced files exist"
        fi
    else
        log_warning "No packages found in $NUPKGS_DIR"
        log_info "This usually means all packaging operations failed."
        log_info "Common solutions:"
        log_info "  - Check NU5017 errors: Ensure projects have content to package"
        log_info "  - Verify project references and dependencies"
        log_info "  - Check if IsPackable=false is set anywhere"
        return 1
    fi
}

# Validate API key
validate_api_key() {
    if [[ -n "$API_KEY" ]]; then
        log_step "Validating API key..."
        
        if [[ ${#API_KEY} -lt 32 ]]; then
            log_warning "API key seems too short. Are you sure it's correct?"
        fi
        
        if [[ "$API_KEY" =~ [[:space:]] ]]; then
            error_exit "API key contains whitespace. Please check your API key."
        fi
        
        log_success "API key validation passed"
    fi
}

# Publish a single package
publish_package() {
    local package_pattern="$1"
    local package_name="$2"
    
    # Expand the pattern to get actual filenames
    local package_files
    package_files=($(find "$NUPKGS_DIR" -name "$package_pattern" -type f 2>/dev/null))
    
    if [[ ${#package_files[@]} -eq 0 ]]; then
        log_warning "Package not found: $package_pattern"
        return 1
    fi
    
    # Process each matching package file
    local failed=false
    for package_file in "${package_files[@]}"; do
        local push_cmd="dotnet nuget push '$package_file' --source '$NUGET_SOURCE' --api-key '$API_KEY' --skip-duplicate"
        
        # Capture output to handle symbol package errors gracefully
        local output
        local exit_code
        
        if [[ "$DRY_RUN" == "true" ]]; then
            log_info "Would execute: $push_cmd"
            continue
        fi
        
        output=$(eval "$push_cmd" 2>&1)
        exit_code=$?
        
        # Check if the main package was successfully pushed even if symbol package failed
        if [[ $exit_code -ne 0 ]]; then
            # Check if the error is only about symbol packages
            if echo "$output" | grep -q "Your package was pushed" && echo "$output" | grep -q "does not contain any symbol"; then
                log_warning "Symbol package upload failed (no .pdb files), but main package was pushed successfully: $(basename "$package_file")"
                if [[ "$VERBOSE" == "true" ]]; then
                    echo "$output"
                fi
            else
                log_error "Failed to publish: $(basename "$package_file")"
                if [[ -n "$output" ]]; then
                    echo "$output" | head -10
                fi
                failed=true
            fi
        else
            if [[ "$VERBOSE" == "true" && -n "$output" ]]; then
                echo "$output"
            fi
        fi
    done
    
    if [[ "$failed" == "true" ]]; then
        return 1
    fi
    
    return 0
}

# Publish projects in parallel if enabled
publish_projects_batch() {
    local packages=("$@")
    local pids=()
    local failed_packages=()
    
    if [[ "$PARALLEL_BUILD" == "true" && ${#packages[@]} -gt 1 ]]; then
        log_info "Publishing ${#packages[@]} packages in parallel..."
        
        for i in $(seq 0 2 $((${#packages[@]} - 1))); do
            local pattern="${packages[$i]}"
            local name="${packages[$i+1]}"
            
            if [[ "$DRY_RUN" == "false" ]]; then
                publish_package "$pattern" "$name" &
                pids+=($!)
            else
                publish_package "$pattern" "$name"
            fi
        done
        
        # Wait for all background processes to complete
        if [[ "$DRY_RUN" == "false" ]]; then
            for i in "${!pids[@]}"; do
                if ! wait "${pids[$i]}"; then
                    failed_packages+=("${packages[$((i*2))]}")
                fi
            done
        fi
    else
        log_info "Publishing ${#packages[@]} packages sequentially..."
        for i in $(seq 0 2 $((${#packages[@]} - 1))); do
            local pattern="${packages[$i]}"
            local name="${packages[$i+1]}"
            
            if ! publish_package "$pattern" "$name"; then
                failed_packages+=("$pattern")
            fi
        done
    fi
    
    if [[ ${#failed_packages[@]} -gt 0 ]]; then
        log_error "Failed to publish the following packages:"
        printf '%s\n' "${failed_packages[@]}"
        return 1
    fi
    
    return 0
}

# Publish all packages in dependency order
publish_all_packages() {
    log_step "Publishing packages to NuGet.org in dependency order..."
    
    # Define packages for each level with their patterns and display names
    local -A LEVEL_PACKAGES
    LEVEL_PACKAGES=(
        [0]="CSharpEssentials.Core.*.nupkg Core CSharpEssentials.Enums.*.nupkg Enums CSharpEssentials.Time.*.nupkg Time CSharpEssentials.Clone.*.nupkg Clone CSharpEssentials.AspNetCore.*.nupkg AspNetCore CSharpEssentials.GcpSecretManager.*.nupkg GcpSecretManager CSharpEssentials.RequestResponseLogging.*.nupkg RequestResponseLogging"
        [1]="CSharpEssentials.Errors.*.nupkg Errors CSharpEssentials.Json.*.nupkg Json"
        [2]="CSharpEssentials.Results.*.nupkg Results"
        [3]="CSharpEssentials.Any.*.nupkg Any CSharpEssentials.Maybe.*.nupkg Maybe CSharpEssentials.Rules.*.nupkg Rules CSharpEssentials.Entity.*.nupkg Entity CSharpEssentials.EntityFrameworkCore.*.nupkg EntityFrameworkCore"
        [4]="CSharpEssentials.[0-9]*.nupkg Main"
    )
    
    local max_level=4
    for level in $(seq 0 $max_level); do
        local packages_str="${LEVEL_PACKAGES[$level]}"
        if [[ -n "$packages_str" ]]; then
            IFS=' ' read -ra packages <<< "$packages_str"
            
            log_progress "Level $level: Publishing packages..."
            
            if ! publish_projects_batch "${packages[@]}"; then
                error_exit "Failed to publish Level $level packages"
            fi
            log_success "Level $level publishing completed"
        fi
    done
}

# Main execution flow
main() {
    echo ""
    log_info "🚀 CSharpEssentials NuGet Package Build and Publish Script v2.0"
    echo ""
    
    # Parse arguments
    parse_arguments "$@"
    
    # Validate environment and setup
    validate_environment
    validate_api_key
    prepare_output_directory
    
    # Build and package
    build_solution
    package_all_projects
    
    # Check if any packages were created before proceeding
    if [[ "$DRY_RUN" == "false" ]] && [[ ! -d "$NUPKGS_DIR" || -z "$(find "$NUPKGS_DIR" -name "*.nupkg" -type f 2>/dev/null)" ]]; then
        log_error "No packages were created. Cannot proceed with publishing."
        log_info "Please fix the packaging errors above and try again."
        exit 1
    fi
    
    list_packages
    
    # Publish if API key is provided
    if [[ -z "$API_KEY" ]]; then
        echo ""
        log_info "💡 To upload packages to NuGet.org:"
        log_info "   $0 [OPTIONS] YOUR_API_KEY"
        log_info "   or use: dotnet nuget push \"$NUPKGS_DIR/*.nupkg\" --source $NUGET_SOURCE --api-key YOUR_API_KEY"
        exit 0
    fi
    
    # Only publish if we have packages to publish
    local package_count
    package_count=$(find "$NUPKGS_DIR" -name "*.nupkg" -type f 2>/dev/null | wc -l)
    
    if [[ $package_count -eq 0 ]]; then
        log_error "No packages available for publishing. Exiting."
        exit 1
    fi
    
    log_info "Found $package_count packages ready for publishing."
    publish_all_packages
    
    echo ""
    log_success "🎉 Process completed successfully!"
}

# Run main function with all arguments
main "$@"