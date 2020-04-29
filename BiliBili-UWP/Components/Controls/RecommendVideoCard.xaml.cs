﻿using BiliBili_Lib.Models.BiliBili;
using BiliBili_UWP.Components.Widgets;
using BiliBili_UWP.Models.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliBili_UWP.Components.Controls
{
    public sealed partial class RecommendVideoCard : UserControl
    {
        public RecommendVideoCard()
        {
            this.InitializeComponent();
        }

        public event EventHandler<VideoRecommend> NeedRemoveVideo;

        public VideoRecommend Data
        {
            get { return (VideoRecommend)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(VideoRecommend), typeof(RecommendVideoCard), new PropertyMetadata(null,new PropertyChangedCallback(Data_Changed)));

        private static void Data_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue!=null && e.NewValue is VideoRecommend data)
            {
                var instance = d as RecommendVideoCard;
                var card = instance.VideoCard;
                card.Cover = data.cover;
                card.Title = data.title;
                card.Tip = string.IsNullOrEmpty(data.args?.up_name) ? "--" : data.args.up_name;
                card.FirstSectionContent = data.cover_left_text_1;
                card.SecondSectionContent = data.cover_left_text_2;
                card.Duration = data.cover_right_text;
                card.RightBottomText = data.rcmd_reason ?? "";
                if (data.card_goto == "bangumi")
                    card.ExtraFlyout = instance.BangumiFlyout;
                else
                    card.ExtraFlyout = instance.VideoFlyout;
                var feedback = data.three_point_v2.Where(p => p.type == "feedback").FirstOrDefault();
                if (feedback != null)
                {
                    instance.FeedbackItemsControl.Visibility = Visibility.Visible;
                    instance.FeedbackItemsControl.ItemsSource = feedback.reasons;
                }
                else
                    instance.FeedbackItemsControl.Visibility = Visibility.Collapsed;
                var notInterested = data.three_point_v2.Where(p => p.type == "dislike").FirstOrDefault();
                if (notInterested != null)
                {
                    instance.NotInterestedItemsControl.Visibility = Visibility.Visible;
                    instance.NotInterestedItemsControl.ItemsSource = notInterested.reasons;
                }
                else
                    instance.NotInterestedItemsControl.Visibility = Visibility.Collapsed;
            }
        }

        private async void LaterViewButton_Click(object sender, RoutedEventArgs e)
        {
            LaterViewButton.IsEnabled = false;
            bool result = await App.BiliViewModel._client.Account.AddViewLaterAsync(Data.args.aid);
            if (result)
            {
                new TipPopup("已添加至稍后观看").ShowMessage();
                VideoFlyout.Hide();
            }
            else
            {
                new TipPopup("添加失败").ShowError();
            }
            LaterViewButton.IsEnabled = true;
        }

        private async void Dislike_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as Reason;
            NotInterestedItemsControl.IsEnabled = false;
            bool result = await App.BiliViewModel._client.Video.DislikeRecommendVideoAsync(Data.args, item.id, Data.card_goto);
            if(result)
            {
                new TipPopup("调整成功，将在下一次优化您的推荐").ShowMessage();
                VideoFlyout.Hide();
                NeedRemoveVideo?.Invoke(this, Data);
            }
            else
            {
                new TipPopup("调整失败，请稍后重试").ShowError();
            }
        }

        private async void Feedback_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as Reason;
            NotInterestedItemsControl.IsEnabled = false;
            bool result = await App.BiliViewModel._client.Video.DislikeRecommendVideoAsync(Data.args, item.id, Data.card_goto,true);
            if (result)
            {
                new TipPopup("调整成功，将在下一次优化您的推荐").ShowMessage();
                VideoFlyout.Hide();
                NeedRemoveVideo?.Invoke(this, Data);
            }
            else
            {
                new TipPopup("调整失败，请稍后重试").ShowError();
            }
        }

        private async void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await App.BiliViewModel._client.Anime.DislikeRecommendVideoAsync(Data.param);
            if (result)
            {
                new TipPopup("调整成功，将在下一次优化您的推荐").ShowMessage();
                VideoFlyout.Hide();
                NeedRemoveVideo?.Invoke(this, Data);
            }
            else
            {
                new TipPopup("调整失败，请稍后重试").ShowError();
            }
        }
    }
}