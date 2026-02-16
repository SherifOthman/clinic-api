# wwwroot - Static Files & Uploads

## Overview

This folder contains static files and user-uploaded content. The folder structure is tracked in Git, but uploaded files are ignored to keep the repository clean and protect user privacy.

## Folder Structure

```
wwwroot/
├── uploads/
│   ├── profiles/          # User profile images
│   │   └── .gitkeep      # Preserves folder in Git
│   └── medical-files/     # Patient medical documents
│       └── .gitkeep      # Preserves folder in Git
└── README.md             # This file
```

## Git Configuration

### What's Tracked

- ✅ Folder structure (via .gitkeep files)
- ✅ This README file

### What's Ignored

- ❌ All uploaded files (_.jpg, _.png, \*.pdf, etc.)
- ❌ User profile images
- ❌ Medical documents
- ❌ Any personal or sensitive files

### .gitignore Rules

```gitignore
# Ignore all uploaded files
wwwroot/uploads/**/*

# But keep folder structure
!wwwroot/uploads/**/.gitkeep
```

## Automatic Directory Creation

The application automatically creates required directories on first upload:

**Service:** `LocalFileStorageService.cs`

```csharp
if (!Directory.Exists(folderPath))
{
    Directory.CreateDirectory(folderPath);
}
```

**When:** Directories are created automatically when:

- First profile image is uploaded
- First medical file is uploaded
- Any file upload operation occurs

## File Upload Configuration

**Location:** `appsettings.json`

```json
{
  "FileStorage": {
    "UploadPath": "wwwroot/uploads",
    "BaseUrl": "/uploads",
    "MaxFileSizeBytes": 5242880, // 5 MB
    "AllowedImageExtensions": [".jpg", ".jpeg", ".png", ".gif"],
    "AllowedDocumentExtensions": [".pdf", ".doc", ".docx"]
  }
}
```

## Deployment

### Development

1. Clone repository
2. Run application
3. Upload files - directories created automatically
4. Files stored locally in wwwroot/uploads/

### Production

1. Deploy application
2. Ensure wwwroot folder has write permissions
3. Configure file storage (local or cloud)
4. Directories created automatically on first upload

### Cloud Storage (Optional)

For production, consider using cloud storage:

- Azure Blob Storage
- AWS S3
- Google Cloud Storage

Implement `IFileStorageService` interface for cloud providers.

## Security Considerations

### File Validation

- ✅ File size limits enforced
- ✅ File extension validation
- ✅ Unique filenames (GUID-based)
- ✅ Path traversal prevention

### Privacy

- ✅ Uploaded files not tracked in Git
- ✅ Personal images excluded from repository
- ✅ Medical documents kept private
- ✅ No sensitive data in version control

### Permissions

**Required:** Write permissions on wwwroot/uploads/

```bash
# Linux/Mac
chmod 755 wwwroot/uploads

# Windows
# Ensure IIS_IUSRS has write permissions
```

## Maintenance

### Cleanup Old Files

Consider implementing:

- Scheduled cleanup of orphaned files
- File retention policies
- Automatic deletion of files from deleted records

### Backup

**Important:** Backup uploaded files separately from database:

- Profile images
- Medical documents
- Any user-generated content

### Monitoring

Monitor disk space usage:

```bash
# Check folder size
du -sh wwwroot/uploads/

# Count files
find wwwroot/uploads/ -type f | wc -l
```

## Troubleshooting

### Issue: Directories not created

**Solution:** Check write permissions on wwwroot folder

### Issue: Files not uploading

**Solution:**

1. Check file size limits in appsettings.json
2. Verify allowed extensions
3. Check disk space
4. Review application logs

### Issue: Files visible in Git

**Solution:**

1. Verify .gitignore is correct
2. Remove tracked files: `git rm --cached wwwroot/uploads/**/*`
3. Commit changes

## Development Notes

### Adding New Upload Types

1. Create new subfolder in uploads/
2. Add .gitkeep file
3. Update FileStorageOptions if needed
4. Implement upload logic in service

### Testing

Test files are NOT included in repository. Use:

- Sample images from https://picsum.photos/
- Test PDFs from https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf

## Summary

✅ Folder structure preserved in Git
✅ Uploaded files excluded from repository
✅ Automatic directory creation on first use
✅ Privacy and security maintained
✅ Clean repository without personal data
