﻿using System.ComponentModel.Composition;

using Ookii.Dialogs.Wpf;

using STR.DialogView.Domain.Messages;

using STR.MvvmCommon;
using STR.MvvmCommon.Contracts;

using UpkManager.Domain.Constants;
using UpkManager.Domain.Messages.Application;

using UpkManager.Wpf.ViewModels.Dialogs;


namespace UpkManager.Wpf.Controllers.Dialogs {

  [Export(typeof(IController))]
  public class SettingsController : IController {

    #region Private Fields

    private string oldPathToGame;

    private readonly SettingsViewModel viewModel;

    private readonly IMessenger messenger;

    #endregion Private Fields

    #region Constructor

    [ImportingConstructor]
    public SettingsController(SettingsViewModel ViewModel, IMessenger Messenger) {
      viewModel = ViewModel;

      messenger = Messenger;

      registerMessages();
      registerCommands();
    }

    #endregion Constructor

    #region Messages

    private void registerMessages() {
      messenger.Register<SettingsEditMessage>(this, onSettingsEdit);
    }

    private void onSettingsEdit(SettingsEditMessage message) {
      viewModel.Message = message;

      oldPathToGame = message.Settings.PathToGame;

      messenger.Send(new OpenDialogMessage { Name = DialogNames.Settings });
    }

    #endregion Messages

    #region Commands

    private void registerCommands() {
      viewModel.SelectGameDir    = new RelayCommand(onSelectGameDir);
      viewModel.SelectExportPath = new RelayCommand(onSelectExportPath);

      viewModel.Ok     = new RelayCommand(onOkExecute);
      viewModel.Cancel = new RelayCommand(onCancelExecute);
    }

    #region SelectGameDir Command

    private void onSelectGameDir() {
      VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();

      bool? result = fbd.ShowDialog();

      if (!result.HasValue || !result.Value) return;

      viewModel.Message.Settings.PathToGame = fbd.SelectedPath;
    }

    #endregion SelectGameDir Command

    #region SelectExportPath Command

    private void onSelectExportPath() {
      VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();

      bool? result = fbd.ShowDialog();

      if (!result.HasValue || !result.Value) return;

      viewModel.Message.Settings.ExportPath = fbd.SelectedPath;
    }

    #endregion SelectExportPath Command

    #region OK Command

    private void onOkExecute() {
      string pathToGame = viewModel.Message.Settings.PathToGame;
      string exportPath = viewModel.Message.Settings.ExportPath;

      if (!pathToGame.ToLowerInvariant().EndsWith("contents") && !pathToGame.ToLowerInvariant().EndsWith(@"contents\")) {
        messenger.Send(new MessageBoxDialogMessage { Header = "Invalid Path", Message = "The selected directory does not contain the 'bns' and 'Local' directories.", HasCancel = false });

        viewModel.Message.Settings.PathToGame = oldPathToGame;

        return;
      }

      messenger.Send(new CloseDialogMessage());

      viewModel.Message.Settings.PathToGame = pathToGame.EndsWith(@"\") ? pathToGame : pathToGame + @"\";
      viewModel.Message.Settings.ExportPath = exportPath.EndsWith(@"\") ? exportPath : exportPath + @"\";

      if (viewModel.Message.Callback != null) {
        viewModel.Message.Callback(viewModel.Message);

        viewModel.Message.Callback = null;
      }
    }

    #endregion OK Command

    #region Cancel Command

    private void onCancelExecute() {
      messenger.Send(new CloseDialogMessage());

      if (viewModel.Message.Callback != null) {
        viewModel.Message.IsCancel = true;

        viewModel.Message.Callback(viewModel.Message);

        viewModel.Message.Callback = null;
      }
    }

    #endregion Cancel Command

    #endregion Commands

  }

}