using Bookano.Application.DTOs.Publishers;
using Bookano.Application.Validators.Common;
using FluentValidation;

namespace Bookano.Application.Validators;

public class PublisherSaveDtoValidator : CommonValidator<PublisherSaveDto>
{
    public PublisherSaveDtoValidator() : base(x => x.Name) { }
}