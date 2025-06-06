using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using JetBrains.Annotations;
using NexusMods.App.UI.Resources;
using ReactiveUI;

namespace NexusMods.App.UI.Controls.Settings.SettingEntries;

[UsedImplicitly]
public partial class SettingEntryView : ReactiveUserControl<ISettingEntryViewModel>
{
    public SettingEntryView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .WhereNotNull()
                .Do(PopulateFromViewModel)
                .Subscribe()
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel!.InteractionControlViewModel.ValueContainer.HasChanged)
                .Select(_ => ViewModel?.InteractionControlViewModel.ValueContainer.CurrentValue)
                .Prepend(ViewModel?.InteractionControlViewModel.ValueContainer.CurrentValue)
                .Select(value =>
                {
                    if (value is null) return string.Empty;
                    return ViewModel?.PropertyUIDescriptor.DescriptionFactory.Invoke(value) ?? string.Empty;
                })
                .SubscribeWithErrorLogging(description => EntryDescription.Text = description)
                .DisposeWith(disposables);

            this.WhenAnyValue(x =>
                    x.ViewModel!.InteractionControlViewModel.ValueContainer.HasChanged,
                    x => x.ViewModel!.PropertyUIDescriptor.RequiresRestart,
                    (hasChanged, requiresRestart) => requiresRestart && hasChanged
                )
                .BindToView(this, view => view.RequiresRestartBanner.IsVisible)
                .DisposeWith(disposables);
        });
    }

    private void PopulateFromViewModel(ISettingEntryViewModel viewModel)
    {
        var descriptor = viewModel.PropertyUIDescriptor;

        InteractionControl.ViewModel = viewModel.InteractionControlViewModel;

        EntryName.Text = descriptor.DisplayName;

        LinkViewModel.ViewModel = viewModel.LinkRenderer;
        LinkViewModel.IsVisible = viewModel.LinkRenderer is not null;

        RequiresRestartMessage.Text = descriptor.RestartMessage ?? Language.SettingEntryView_NeedRestartMessage;
    }
}
