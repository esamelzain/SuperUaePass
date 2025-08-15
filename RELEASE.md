# Release Guide

This document explains how to use the automated GitHub Actions workflows for building, testing, and publishing the SuperUaePass package to NuGet.

## 🚀 Automated Release Process

### Prerequisites

1. **NuGet API Key**: You need to add your NuGet API key as a GitHub secret
   - Go to your GitHub repository → Settings → Secrets and variables → Actions
   - Add a new secret named `NUGET_API_KEY` with your NuGet API key value

2. **GitHub Token**: The workflows use `GITHUB_TOKEN` which is automatically provided by GitHub Actions

### Workflows

#### 1. CI Workflow (`ci.yml`)
- **Triggers**: Push to master/main branch, Pull requests
- **Purpose**: Build, test, and create package artifacts
- **Actions**: 
  - Restores dependencies
  - Builds the project
  - Runs tests
  - Creates NuGet package
  - Uploads package as artifact

#### 2. Release Workflow (`release.yml`)
- **Triggers**: Push tags starting with `v*` (e.g., `v1.1.0`)
- **Purpose**: Publish to NuGet and create GitHub release
- **Actions**:
  - Builds and tests the project
  - Publishes package to NuGet
  - Creates GitHub release with release notes

## 📦 How to Release a New Version

### Method 1: Tag-based Release (Recommended)

1. **Update Version**: Manually update the version in `src/SuperUaePass/SuperUaePass.csproj`
   ```xml
   <Version>1.1.0</Version>
   ```

2. **Commit Changes**: Commit your changes to master branch
   ```bash
   git add .
   git commit -m "Update version to 1.1.0"
   git push origin master
   ```

3. **Create and Push Tag**: Create a tag for the release
   ```bash
   git tag v1.1.0
   git push origin v1.1.0
   ```

4. **Automated Release**: The workflow will automatically:
   - Build the project
   - Run tests
   - Publish to NuGet
   - Create GitHub release

### Method 2: Direct Master Push (Alternative)

The `build-and-publish.yml` workflow will also publish to NuGet when you push directly to master branch, but this is less controlled.

## 🔧 Workflow Configuration

### Environment Variables
- `DOTNET_VERSION`: Set to `8.0.x` for consistent builds
- `NUGET_PACKAGE_NAME`: Set to `SuperUaePass`

### Secrets Required
- `NUGET_API_KEY`: Your NuGet API key for publishing packages

## 📋 Release Checklist

Before creating a release:

- [ ] Update version in `SuperUaePass.csproj`
- [ ] Update release notes in the workflow or create a separate release notes file
- [ ] Test locally: `dotnet build`, `dotnet test`, `dotnet pack`
- [ ] Commit all changes to master branch
- [ ] Create and push version tag
- [ ] Verify the workflow runs successfully
- [ ] Check NuGet for the published package
- [ ] Verify GitHub release is created

## 🐛 Troubleshooting

### Common Issues

1. **NuGet API Key Error**
   - Ensure `NUGET_API_KEY` secret is set in GitHub repository settings
   - Verify the API key is valid and has publish permissions

2. **Build Failures**
   - Check the workflow logs for specific error messages
   - Ensure all dependencies are properly referenced
   - Verify the project builds locally

3. **Package Already Exists**
   - The workflow uses `--skip-duplicate` flag to handle this
   - Ensure version number is unique

4. **GitHub Release Creation Fails**
   - Check that `GITHUB_TOKEN` has sufficient permissions
   - Verify the tag format is correct (`v*`)

### Manual Release

If automated release fails, you can manually:

1. Build and pack locally:
   ```bash
   dotnet build --configuration Release
   dotnet pack --configuration Release
   ```

2. Publish to NuGet:
   ```bash
   dotnet nuget push bin/Release/SuperUaePass.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
   ```

3. Create GitHub release manually through the GitHub web interface

## 📈 Version Management

### Semantic Versioning
- **Major**: Breaking changes (1.0.0 → 2.0.0)
- **Minor**: New features, backward compatible (1.0.0 → 1.1.0)
- **Patch**: Bug fixes, backward compatible (1.0.0 → 1.0.1)

### Version Update Process
1. Update version in `src/SuperUaePass/SuperUaePass.csproj`
2. Update release notes in the workflow file
3. Commit and tag the release
4. Let the automation handle the rest

## 🔒 Security

- Never commit API keys to the repository
- Use GitHub secrets for sensitive information
- Regularly rotate your NuGet API key
- Review workflow permissions and access

## 📞 Support

If you encounter issues with the release process:

1. Check the GitHub Actions logs for detailed error messages
2. Verify all prerequisites are met
3. Test the build process locally
4. Create an issue in the repository for persistent problems
