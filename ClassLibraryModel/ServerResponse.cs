using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryModel
{
    public record ServerResponse(
        string? token,
        string? error,
        UserModel? userModelResponse,
        string? stringResponse,
        bool? boolResponse,
        FormModel? formModelResponse,
        List<FormModel>? formsModelResponse,
        Submission? submissionModelResponse,
        List<Submission>? submissionsModelResponse);
}
